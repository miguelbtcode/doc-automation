// Slash command extension — permite tipear "/" en el editor para abrir un menú
// con comandos rápidos (insertar headings, paneles, listas, tablas, etc.)
//
// Implementado sobre @tiptap/suggestion + tippy.js (mismo patrón que Notion).

import { Extension } from '@tiptap/core';
import Suggestion from '@tiptap/suggestion';
import tippy from 'tippy.js';
import { iconHtml } from './icons.js';

const COMMAND_ITEMS = [
    {
        title: 'Título 1',
        description: 'Encabezado grande',
        icon: 'textStyle',
        keywords: ['h1', 'heading', 'titulo'],
        command: ({ editor, range }) => editor.chain().focus().deleteRange(range).setNode('heading', { level: 1 }).run()
    },
    {
        title: 'Título 2',
        description: 'Encabezado mediano',
        icon: 'textStyle',
        keywords: ['h2', 'heading', 'subtitulo'],
        command: ({ editor, range }) => editor.chain().focus().deleteRange(range).setNode('heading', { level: 2 }).run()
    },
    {
        title: 'Título 3',
        description: 'Encabezado pequeño',
        icon: 'textStyle',
        keywords: ['h3', 'heading'],
        command: ({ editor, range }) => editor.chain().focus().deleteRange(range).setNode('heading', { level: 3 }).run()
    },
    {
        title: 'Lista con viñetas',
        description: 'Lista no ordenada',
        icon: 'bulletList',
        keywords: ['ul', 'bullet', 'lista'],
        command: ({ editor, range }) => editor.chain().focus().deleteRange(range).toggleBulletList().run()
    },
    {
        title: 'Lista numerada',
        description: 'Lista ordenada',
        icon: 'numberList',
        keywords: ['ol', 'numbered', 'lista'],
        command: ({ editor, range }) => editor.chain().focus().deleteRange(range).toggleOrderedList().run()
    },
    {
        title: 'Tabla',
        description: 'Insertar tabla 3x3',
        icon: 'table',
        keywords: ['table', 'tabla'],
        command: ({ editor, range }) => editor.chain().focus().deleteRange(range)
            .insertTable({ rows: 3, cols: 3, withHeaderRow: true }).run()
    },
    {
        title: 'Bloque de código',
        description: 'Insertar code block',
        icon: 'code',
        keywords: ['code', 'codigo'],
        command: ({ editor, range }) => editor.chain().focus().deleteRange(range).toggleCodeBlock().run()
    },
    {
        title: 'Línea horizontal',
        description: 'Separador',
        icon: 'horizontalRule',
        keywords: ['hr', 'separator', 'divider'],
        command: ({ editor, range }) => editor.chain().focus().deleteRange(range).setHorizontalRule().run()
    },
    // Paneles tipo Jira
    {
        title: 'Panel Info',
        description: 'Nota informativa (azul)',
        icon: 'info',
        iconColor: '#0747A6',
        keywords: ['info', 'panel', 'nota'],
        command: ({ editor, range }) => editor.chain().focus().deleteRange(range).setPanel('info').run()
    },
    {
        title: 'Panel Note',
        description: 'Nota general (violeta)',
        icon: 'note',
        iconColor: '#403294',
        keywords: ['note', 'panel', 'nota'],
        command: ({ editor, range }) => editor.chain().focus().deleteRange(range).setPanel('note').run()
    },
    {
        title: 'Panel Success',
        description: 'Confirmación (verde)',
        icon: 'success',
        iconColor: '#006644',
        keywords: ['success', 'panel', 'exito'],
        command: ({ editor, range }) => editor.chain().focus().deleteRange(range).setPanel('success').run()
    },
    {
        title: 'Panel Warning',
        description: 'Advertencia (amarillo)',
        icon: 'warning',
        iconColor: '#FF8B00',
        keywords: ['warning', 'panel', 'advertencia'],
        command: ({ editor, range }) => editor.chain().focus().deleteRange(range).setPanel('warning').run()
    },
    {
        title: 'Panel Error',
        description: 'Error / riesgo (rojo)',
        icon: 'error',
        iconColor: '#BF2600',
        keywords: ['error', 'panel', 'riesgo'],
        command: ({ editor, range }) => editor.chain().focus().deleteRange(range).setPanel('error').run()
    }
];

function filterItems(query) {
    const q = query.toLowerCase();
    if (!q) return COMMAND_ITEMS;
    return COMMAND_ITEMS.filter(item =>
        item.title.toLowerCase().includes(q) ||
        item.keywords.some(k => k.includes(q))
    );
}

function renderItems() {
    let popup;
    let component;
    let selectedIndex = 0;

    const renderList = (props) => {
        const container = document.createElement('div');
        container.className = 'da-slash-menu';

        if (props.items.length === 0) {
            container.innerHTML = '<div class="da-slash-empty">Sin resultados</div>';
            return container;
        }

        props.items.forEach((item, index) => {
            const el = document.createElement('button');
            el.type = 'button';
            el.className = 'da-slash-item' + (index === selectedIndex ? ' da-slash-item-selected' : '');
            const colorStyle = item.iconColor ? ` style="color: ${item.iconColor};"` : '';
            el.innerHTML = `
                <span class="da-slash-icon"${colorStyle}>${iconHtml(item.icon, 18)}</span>
                <span class="da-slash-body">
                    <span class="da-slash-title">${item.title}</span>
                    <span class="da-slash-desc">${item.description}</span>
                </span>
            `;
            el.addEventListener('click', () => props.command(item));
            container.appendChild(el);
        });

        return container;
    };

    return {
        onStart: (props) => {
            selectedIndex = 0;
            component = renderList(props);

            popup = tippy('body', {
                getReferenceClientRect: props.clientRect,
                appendTo: () => document.body,
                content: component,
                showOnCreate: true,
                interactive: true,
                trigger: 'manual',
                placement: 'bottom-start',
                theme: 'light-border',
                maxWidth: 320
            })[0];
        },
        onUpdate: (props) => {
            selectedIndex = 0;
            const newComponent = renderList(props);
            popup?.setContent(newComponent);
            component = newComponent;
            popup?.setProps({ getReferenceClientRect: props.clientRect });
        },
        onKeyDown: (props) => {
            if (props.event.key === 'Escape') {
                popup?.hide();
                return true;
            }
            if (props.event.key === 'ArrowUp') {
                selectedIndex = (selectedIndex + props.items.length - 1) % props.items.length;
                rerender(props);
                return true;
            }
            if (props.event.key === 'ArrowDown') {
                selectedIndex = (selectedIndex + 1) % props.items.length;
                rerender(props);
                return true;
            }
            if (props.event.key === 'Enter') {
                const item = props.items[selectedIndex];
                if (item) {
                    props.command(item);
                }
                return true;
            }
            return false;
        },
        onExit: () => {
            popup?.destroy();
        }
    };

    function rerender(props) {
        const newComponent = renderList(props);
        popup?.setContent(newComponent);
        component = newComponent;
    }
}

export const SlashCommands = Extension.create({
    name: 'slashCommands',

    addOptions() {
        return {
            suggestion: {
                char: '/',
                startOfLine: false,
                command: ({ editor, range, props }) => {
                    props.command({ editor, range });
                },
                items: ({ query }) => filterItems(query),
                render: renderItems
            }
        };
    },

    addProseMirrorPlugins() {
        return [
            Suggestion({
                editor: this.editor,
                ...this.options.suggestion
            })
        ];
    }
});
