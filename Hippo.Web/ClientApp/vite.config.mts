/// <reference types="vitest" />
import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
// import { readFileSync } from "fs";
// import { certFilePath, keyFilePath } from "./aspnetcore-https.js";

// const proxySettings = {
//   target: "https://localhost:7256/",
//   changeOrigin: false,
//   secure: false,
// };

// https://vitejs.dev/config/
export default defineConfig({
  build: {
    outDir: "build",
  },
  plugins: [react()],
  test: {
    globals: true,
    root: __dirname,
    setupFiles: "./vitest.setup.ts",
    workspace: [
      {
        extends: true,
        test: {
          include: ["src/**/*.test.{ts,tsx}"],
          environment: "jsdom",
        },
      },
    ],
    coverage: {
      reporter: ["cobertura", "text"],
    },
  },
  server: {
    // https: {
    //   key: process.env.VITEST_ENV !== "ci" ? readFileSync(keyFilePath) : "",
    //   cert: process.env.VITEST_ENV !== "ci" ? readFileSync(certFilePath) : "",
    // },
    port: 3000,
    strictPort: true,
    // proxy: {
    //   "/weatherforecast": { ...proxySettings },
    //   "/account/login": { ...proxySettings },
    //   "/api": { ...proxySettings },
    //   "/signin-oidc": { ...proxySettings },
    //   "/signout-oidc": { ...proxySettings },
    //   "/signout-callback-oidc": { ...proxySettings },
    //   "/system": { ...proxySettings },
    // },
  },
});
