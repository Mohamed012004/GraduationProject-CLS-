import { defineConfig, loadEnv } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '');

  return {
    plugins: [react()],
    server: {
      port: 5173,
      host: '0.0.0.0', // Restored this from your package.json dev script
      watch: {
        usePolling: true, // Forces manual file checking to bypass the ENOSPC error
        interval: 100,    // Checks files for updates every 100 milliseconds
      },
      proxy: {
        '/api': {
          target: env.VITE_API_BASE_URL || 'https://localhost:7098',
          changeOrigin: true,
          secure: false,
        },
      },
    },
  };
});
