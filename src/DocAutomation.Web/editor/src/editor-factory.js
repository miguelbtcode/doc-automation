// Editor factory — crea instancia de Tiptap con todas las extensiones

import { Editor } from '@tiptap/core';
import StarterKit from '@tiptap/starter-kit';
import Link from '@tiptap/extension-link';
import Image from '@tiptap/extension-image';
import Table from '@tiptap/extension-table';
import TableRow from '@tiptap/extension-table-row';
import TableCell from '@tiptap/extension-table-cell';
import TableHeader from '@tiptap/extension-table-header';
import TextStyle from '@tiptap/extension-text-style';
import Color from '@tiptap/extension-color';
import Highlight from '@tiptap/extension-highlight';
import Underline from '@tiptap/extension-underline';
import Subscript from '@tiptap/extension-subscript';
import Superscript from '@tiptap/extension-superscript';
import TextAlign from '@tiptap/extension-text-align';
import Placeholder from '@tiptap/extension-placeholder';
import BubbleMenu from '@tiptap/extension-bubble-menu';

import { PanelNode, PANEL_TYPES, PANEL_META } from './panel-node.js';
import { SlashCommands } from './slash-commands.js';
import { buildToolbar } from './toolbar.js';
import { buildPanelBubbleMenu } from './bubble-menu.js';

// ── Global type picker (portal en document.body) ─────────────────────────────
// Vive fuera de cualquier overflow/transform/stacking-context del editor.
// Es creado UNA VEZ por instancia de editor y destruido junto con ella.
function createGlobalPicker(editor) {
    const el = document.createElement('div');
    el.className = 'da-panel-type-picker';

    // El icono que abrió el picker. Se usa para resolver la posición del panel
    // en el momento del click del item (no al abrir, así sobrevive a cambios del doc).
    let activeIconEl = null;

    function applyType(typeKey) {
        if (!activeIconEl) return;
        const wrapper = activeIconEl.closest('.da-panel-wrapper');
        const contentEl = wrapper?.querySelector('.da-panel-content');
        if (!contentEl) return;
        try {
            // posAtDOM(contentDOM, 0) → posición al inicio del contenido del panel.
            // El nodo panel arranca una posición antes (la "abertura" del nodo).
            const contentStart = editor.view.posAtDOM(contentEl, 0);
            const panelPos = contentStart - 1;
            const tr = editor.state.tr.setNodeMarkup(panelPos, undefined, { panelType: typeKey });
            editor.view.dispatch(tr);
        } catch (err) {
            console.warn('[da-panel] no se pudo actualizar el tipo:', err);
        }
    }

    PANEL_TYPES.forEach(typeKey => {
        const m = PANEL_META[typeKey];
        const btn = document.createElement('button');
        btn.type = 'button';
        btn.className = 'da-pt-item';
        btn.dataset.type = typeKey;
        btn.innerHTML = `
            <span class="da-pt-icon material-icons" style="color:${m.color}">${m.icon}</span>
            <span class="da-pt-label">${m.label}</span>
        `;
        btn.addEventListener('mousedown', (e) => {
            e.preventDefault();
            e.stopPropagation();
            applyType(typeKey);
            close();
        });
        el.appendChild(btn);
    });

    document.body.appendChild(el);

    // Cierra el picker ante cualquier evento que invalide su posición o foco.
    // capture:true para enterarnos del scroll de cualquier ancestro (overflow-y:auto del editor incluido).
    const onDismiss = () => close();

    function open(iconEl) {
        activeIconEl = iconEl;
        const panel = iconEl.closest('[data-panel-type]');
        const currentType = panel?.getAttribute('data-panel-type') ?? 'info';
        el.querySelectorAll('.da-pt-item').forEach(item =>
            item.classList.toggle('da-pt-active', item.dataset.type === currentType));
        const rect = iconEl.getBoundingClientRect();
        el.style.top  = (rect.bottom + 4) + 'px';
        el.style.left = rect.left + 'px';
        el.classList.add('da-panel-type-picker-open');
        window.addEventListener('scroll', onDismiss, true);
        window.addEventListener('resize', onDismiss);
        window.addEventListener('blur', onDismiss);
    }

    function close() {
        if (!el.classList.contains('da-panel-type-picker-open')) return;
        el.classList.remove('da-panel-type-picker-open');
        activeIconEl = null;
        window.removeEventListener('scroll', onDismiss, true);
        window.removeEventListener('resize', onDismiss);
        window.removeEventListener('blur', onDismiss);
    }

    function toggle(iconEl) {
        el.classList.contains('da-panel-type-picker-open') ? close() : open(iconEl);
    }

    function isOpen() {
        return el.classList.contains('da-panel-type-picker-open');
    }

    function contains(target) {
        return el.contains(target);
    }

    function destroy() {
        el.remove();
    }

    return { open, close, toggle, isOpen, contains, destroy };
}

export function createEditor({ host, initialContent, onChange, placeholder }) {
    host.classList.add('da-editor');
    host.innerHTML = '';

    const toolbarEl = document.createElement('div');
    toolbarEl.className = 'da-editor-toolbar';
    host.appendChild(toolbarEl);

    const editableHost = document.createElement('div');
    editableHost.className = 'da-editor-content';
    host.appendChild(editableHost);

    const bubbleMenuEl = document.createElement('div');
    bubbleMenuEl.className = 'da-editor-bubble-menu';
    bubbleMenuEl.style.display = 'none';
    host.appendChild(bubbleMenuEl);

    const editor = new Editor({
        element: editableHost,
        extensions: [
            StarterKit.configure({
                heading: { levels: [1, 2, 3, 4] },
                codeBlock: { HTMLAttributes: { class: 'da-codeblock' } }
            }),
            Underline,
            Subscript,
            Superscript,
            TextStyle,
            Color,
            Highlight.configure({ multicolor: true }),
            TextAlign.configure({ types: ['heading', 'paragraph'] }),
            Link.configure({
                openOnClick: false,
                HTMLAttributes: { rel: 'noopener', target: '_blank' }
            }),
            Image.configure({ inline: false }),
            Table.configure({ resizable: true }),
            TableRow,
            TableHeader,
            TableCell,
            Placeholder.configure({
                placeholder: placeholder || 'Empezá a escribir... usá "/" para comandos rápidos'
            }),
            PanelNode,
            SlashCommands,
            BubbleMenu.configure({
                element: bubbleMenuEl,
                shouldShow: ({ editor, state }) => {
                    const { selection } = state;
                    const { $from, empty } = selection;
                    if (empty) return false;
                    const isPanel = state.doc.resolve($from.pos).parent.type.name === 'panel';
                    if (isPanel) return true;
                    return editor.isActive('panel');
                },
                tippyOptions: { placement: 'top', theme: 'light-border' }
            })
        ],
        content: initialContent || '',
        onUpdate: ({ editor }) => { if (onChange) onChange(editor.getHTML()); }
    });

    const picker = createGlobalPicker(editor);

    // Capture-phase mousedown — fires before ProseMirror intercepts non-editable elements.
    editableHost.addEventListener('mousedown', (e) => {
        const icon = e.target.closest('.da-panel-icon');
        if (icon) {
            e.preventDefault();
            e.stopPropagation();
            picker.toggle(icon);
            return;
        }
        // Close picker when clicking anywhere else inside the editor (not inside picker)
        if (!picker.contains(e.target)) picker.close();
    }, true);

    // Close picker on clicks outside the editor
    document.addEventListener('mousedown', (e) => {
        if (!editableHost.contains(e.target) && !picker.contains(e.target)) {
            picker.close();
        }
    });

    buildToolbar(toolbarEl, editor);
    buildPanelBubbleMenu(bubbleMenuEl, editor);

    return editor;
}
