import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      "/api": { target: "https://localhost:7117", changeOrigin: true, secure: false },
      "/account": { target: "https://localhost:7117", changeOrigin: true, secure: false },
      "/downloads": { target: "https://localhost:7117", changeOrigin: true, secure: false },
    },
  },
  build: {
    outDir: "../AccessibleSchoolReports.Web/wwwroot",
    emptyOutDir: false,
  },
});
