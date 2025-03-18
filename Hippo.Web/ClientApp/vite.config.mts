/// <reference types="vitest" />
import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

// https://vitejs.dev/config/
export default defineConfig({
  build: {
    outDir: "build",
    rollupOptions: {
      input: "src/index.tsx",
    },
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
    port: 3000,
    strictPort: true,
  },
});
