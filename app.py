# -*- coding: utf-8 -*-
from flask import Flask, request, jsonify
from flask_cors import CORS
import face_recognition
import os
import base64
import cv2
import numpy as np
import mediapipe as mp
from io import BytesIO
from PIL import Image

app = Flask(__name__)
CORS(app)

mp_face_mesh = mp.solutions.face_mesh

REGISTERED_DIR = r"D:\C#\QLCafeHV\QLCafeHV\wwwroot\facesregister"

if not os.path.exists(REGISTERED_DIR):
    os.makedirs(REGISTERED_DIR)

# =========================
# Base64 → image
# =========================
def base64_to_image(image_base64):
    image_data = base64.b64decode(image_base64.split(',')[1])
    img = Image.open(BytesIO(image_data))
    return cv2.cvtColor(np.array(img), cv2.COLOR_RGB2BGR)

# =========================
# Save image
# =========================
def save_base64_image(image_base64, path):
    image_data = base64.b64decode(image_base64.split(',')[1])

    with open(path, "wb") as f:
        f.write(image_data)

# =========================
# Eye ratio
# =========================
def eye_aspect_ratio(points, eye):
    p1 = points[eye[0]]
    p2 = points[eye[1]]
    p3 = points[eye[2]]
    p4 = points[eye[3]]
    p5 = points[eye[4]]
    p6 = points[eye[5]]

    vertical1 = np.linalg.norm(np.array(p2) - np.array(p6))
    vertical2 = np.linalg.norm(np.array(p3) - np.array(p5))
    horizontal = np.linalg.norm(np.array(p1) - np.array(p4))

    return (vertical1 + vertical2) / (2.0 * horizontal)

# =========================
# Eye state
# =========================
def get_eye_state(image_base64):
    frame = base64_to_image(image_base64)

    with mp_face_mesh.FaceMesh(static_image_mode=False) as face_mesh:
        results = face_mesh.process(cv2.cvtColor(frame, cv2.COLOR_BGR2RGB))

        if not results.multi_face_landmarks:
            return None

        landmarks = results.multi_face_landmarks[0].landmark

        h, w, _ = frame.shape
        points = [(int(p.x * w), int(p.y * h)) for p in landmarks]

        left_eye = [33,160,158,133,153,144]
        right_eye = [362,385,387,263,373,380]

        left = eye_aspect_ratio(points, left_eye)
        right = eye_aspect_ratio(points, right_eye)

        ear = (left + right) / 2

        threshold = 0.23

        print("EAR =", ear)

        return "closed" if ear < threshold else "open"

# =========================
# Blink check
# =========================
def detect_blink_3frames(frames):
    states = []

    for frame in frames:
        state = get_eye_state(frame)

        if state is None:
            return False

        states.append(state)

    print("States =", states)

    return states.count("closed") >= 1

# =========================
# Same face
# =========================
def same_face(frames):
    encodings = []

    for frame in frames:
        img = base64_to_image(frame)
        rgb = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)

        face_enc = face_recognition.face_encodings(rgb)

        if len(face_enc) == 0:
            return False

        encodings.append(face_enc[0])

    same1 = face_recognition.compare_faces([encodings[0]], encodings[1], tolerance=0.4)[0]
    same2 = face_recognition.compare_faces([encodings[1]], encodings[2], tolerance=0.4)[0]

    return same1 and same2

# =========================
# Core anti fake logic
# =========================
def process_face_frames(frames):
    if not detect_blink_3frames(frames):
        return False, "Vui lòng chớp mắt"

    if not same_face(frames):
        return False, "Phát hiện đổi khuôn mặt"

    first = frames[0]
    image = base64_to_image(first)

    encodings = face_recognition.face_encodings(image)

    if len(encodings) == 0:
        return False, "Không nhận diện được khuôn mặt"

    return True, first

# =========================
# Compare registered face
# =========================
def verify_face(employee_id, image_base64):
    registered_path = os.path.join(REGISTERED_DIR, f"{employee_id}.jpg")

    if not os.path.exists(registered_path):
        return False, "Chưa có ảnh đăng ký"

    known_image = face_recognition.load_image_file(registered_path)
    unknown_image = base64_to_image(image_base64)

    known_encoding = face_recognition.face_encodings(known_image)
    unknown_encoding = face_recognition.face_encodings(unknown_image)

    if len(known_encoding) == 0:
        return False, "Ảnh đăng ký không có khuôn mặt"

    if len(unknown_encoding) == 0:
        return False, "Không nhận diện được mặt"

    distance = face_recognition.face_distance(
        [known_encoding[0]],
        unknown_encoding[0]
    )[0]

    print("Distance =", distance)

    result = distance < 0.4

    return result, "Khuôn mặt đúng" if result else "Khuôn mặt sai"

# =========================
# Register face
# =========================
@app.route("/register-face", methods=["POST"])
def register_face():
    data = request.json

    employee_id = data.get("employeeId")
    frames = data.get("frames")

    if not employee_id or not frames:
        return jsonify({
            "success": False,
            "message": "Thiếu dữ liệu"
        })

    ok, result = process_face_frames(frames)

    if not ok:
        return jsonify({
            "success": False,
            "message": result
        })

    path = os.path.join(REGISTERED_DIR, f"{employee_id}.jpg")

    save_base64_image(result, path)

    return jsonify({
        "success": True,
        "message": "Đăng ký thành công"
    })

# =========================
# Verify face
# =========================
@app.route("/verify-face", methods=["POST"])
def verify():
    data = request.json

    employee_id = data.get("employeeId")
    frames = data.get("frames")

    if not employee_id or not frames:
        return jsonify({
            "success": False,
            "message": "Thiếu dữ liệu"
        })

    ok, result = process_face_frames(frames)

    if not ok:
        return jsonify({
            "success": False,
            "message": result
        })

    for frame in frames:
        match, msg = verify_face(employee_id, frame)

        if match:
            return jsonify({
                "success": True,
                "message": msg
            })

    return jsonify({
        "success": False,
        "message": "Khuôn mặt không khớp"
    })

# =========================
# Run
# =========================
if __name__ == "__main__":
    app.run(host="127.0.0.1", port=5000, debug=False)