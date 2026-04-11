// esbuild bundler para el editor Tiptap.
// Genera dos archivos en wwwroot/lib/editor/:
//   - editor.bundle.js  (IIFE expone window.tiptapEditor)
//   - editor.bundle.css (estilos del editor + paneles tipo Jira)
//
// Uso:
//   node esbuild.config.js          → build single
//   node esbuild.config.js --watch  → watch mode (rebuildea en cambios)

import * as esbuild from 'esbuild';
import { fileURLToPath } from 'node:url';
import path from 'node:path';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const watchMode = process.argv.includes('--watch');

const outDir = path.resolve(__dirname, '..', 'wwwroot', 'lib', 'editor');

const config = {
    entryPoints: [path.resolve(__dirname, 'src', 'index.js')],
    bundle: true,
    format: 'iife',
    globalName: 'tiptapEditor',
    outfile: path.join(outDir, 'editor.bundle.js'),
    minify: true,
    sourcemap: false,
    target: ['es2020'],
    loader: {
        '.css': 'css',
        '.svg': 'text'
    },
    logLevel: 'info'
};

if (watchMode) {
    const ctx = await esbuild.context(config);
    await ctx.watch();
    console.log('[editor] Watching for changes...');
} else {
    const result = await esbuild.build(config);
    if (result.errors.length > 0) {
        console.error('[editor] Build failed:', result.errors);
        process.exit(1);
    }
    console.log('[editor] Build completed:', config.outfile);
}
