tailwind.config = {
    darkMode: "class",
    theme: {
        extend: {
            colors: {
                primary: "#765a1f",
                secondary: "#77574d",

                surface: "#fbf9f4",
                background: "#fbf9f4",

                "on-surface": "#1b1c19",
                "on-surface-variant": "#4d4639",

                "surface-container-low": "#f5f3ee",
                "surface-container": "#f0eee9",

                "primary-container": "#c6a361",
                "on-primary": "#ffffff"
            },

            borderRadius: {
                DEFAULT: "0px",
                lg: "0px",
                xl: "0px",
                full: "9999px"
            },

            fontFamily: {
                headline: ["Manrope"],
                body: ["Manrope"],
                label: ["Manrope"]
            }
        }
    }
}