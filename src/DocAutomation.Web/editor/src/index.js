// Entry point del bundle del editor — expone la API pública en `window.tiptapEditor`
// para que Blazor pueda crear, leer y destruir editores vía JS interop.
//
// Uso desde Blazor (TiptapEditor.razor):
//   const id = window.tiptapEditor.create(elementRef, initialHtml, dotNetRef);
//   const html = window.tiptapEditor.getHtml(id);
//   window.tiptapEditor.setHtml(id, newHtml);
//   window.tiptapEditor.destroy(id);

import 'tippy.js/dist/tippy.css';
import 'tippy.js/themes/light-border.css';
import './styles.css';

import { createEditor } from './editor-factory.js';

const instances = new Map();

function generateId() {
    return 'tt-' + Math.random().toString(36).slice(2, 11);
}

export function create(host, initialContent, dotNetRef, placeholder) {
    if (!host) {
        console.error('[tiptapEditor] host element is null');
        return null;
    }

    const id = generateId();

    const editor = createEditor({
        host,
        initialContent: initialContent || '',
        placeholder,
        onChange: (html) => {
            // Avisamos a Blazor del cambio para sincronizar el bind
            if (dotNetRef) {
                dotNetRef.invokeMethodAsync('OnContentChanged', html).catch(err => {
                    // El componente puede haberse destruido — silently ignore
                    if (!String(err).includes('disposed')) {
                        console.warn('[tiptapEditor] onChange callback failed:', err);
                    }
                });
            }
        }
    });

    instances.set(id, { editor, dotNetRef });
    return id;
}

export function getHtml(id) {
    const inst = instances.get(id);
    return inst ? inst.editor.getHTML() : '';
}

export function setHtml(id, html) {
    const inst = instances.get(id);
    if (inst && inst.editor.getHTML() !== html) {
        inst.editor.commands.setContent(html || '', false);
    }
}

export function destroy(id) {
    const inst = instances.get(id);
    if (inst) {
        try { inst.editor.destroy(); } catch { /* ignore */ }
        instances.delete(id);
    }
}

export function isReady() {
    return true;
}
