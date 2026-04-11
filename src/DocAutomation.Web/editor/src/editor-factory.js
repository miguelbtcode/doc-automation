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

import { PanelNode } from './panel-node.js';
import { SlashCommands } from './slash-commands.js';
import { buildToolbar } from './toolbar.js';
import { buildPanelBubbleMenu } from './bubble-menu.js';

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

    // Click en wrapper del panel para seleccionarlo
    editableHost.addEventListener('click', (e) => {
        const panelWrapper = e.target.closest('.da-panel');
        if (panelWrapper && !e.target.closest('.da-panel-content *')) {
            const pos = editor.state.doc.resolve(
                editor.view.posAtDOM(panelWrapper, 0)
            );
            editor.chain().focus().setTextSelection(pos).run();
        }
    });

    buildToolbar(toolbarEl, editor);
    buildPanelBubbleMenu(bubbleMenuEl, editor);

    return editor;
}
