// Toolbar fija arriba del editor — botones agrupados por categoría con separadores.
// Usa los SVGs reales de @atlaskit/icon (Apache 2.0) para look pixel-perfect con Jira.

import { iconHtml } from './icons.js';

const PANEL_TYPES = [
    { key: 'info', icon: 'info', label: 'Info' },
    { key: 'note', icon: 'note', label: 'Note' },
    { key: 'success', icon: 'success', label: 'Success' },
    { key: 'warning', icon: 'warning', label: 'Warning' },
    { key: 'error', icon: 'error', label: 'Error' }
];

export function buildToolbar(container, editor) {
    container.innerHTML = '';

    // Grupo: Undo / Redo
    addIconButton(container, 'undo', 'undo', 'Deshacer (Ctrl+Z)',
        () => editor.chain().focus().undo().run());
    addIconButton(container, 'redo', 'redo', 'Rehacer (Ctrl+Y)',
        () => editor.chain().focus().redo().run());
    addSeparator(container);

    // Grupo: Headings (dropdown)
    addHeadingDropdown(container, editor);
    addSeparator(container);

    // Grupo: Inline formatting
    addToggleButton(container, 'bold', 'bold', 'Negrita (Ctrl+B)', editor, 'bold',
        () => editor.chain().focus().toggleBold().run());
    addToggleButton(container, 'italic', 'italic', 'Itálica (Ctrl+I)', editor, 'italic',
        () => editor.chain().focus().toggleItalic().run());
    addToggleButton(container, 'underline', 'underline', 'Subrayado (Ctrl+U)', editor, 'underline',
        () => editor.chain().focus().toggleUnderline().run());
    addToggleButton(container, 'strike', 'strike', 'Tachado', editor, 'strike',
        () => editor.chain().focus().toggleStrike().run());
    addToggleButton(container, 'code', 'code', 'Código inline', editor, 'code',
        () => editor.chain().focus().toggleCode().run());
    addSeparator(container);

    // Grupo: Color y highlight
    addColorPicker(container, editor);
    addHighlightPicker(container, editor);
    addIconButton(container, 'remove-format', 'removeFormatting', 'Quitar formato',
        () => editor.chain().focus().unsetAllMarks().clearNodes().run());
    addSeparator(container);

    // Grupo: Listas
    addToggleButton(container, 'bullet', 'bulletList', 'Lista con viñetas', editor, 'bulletList',
        () => editor.chain().focus().toggleBulletList().run());
    addToggleButton(container, 'ordered', 'numberList', 'Lista numerada', editor, 'orderedList',
        () => editor.chain().focus().toggleOrderedList().run());
    addSeparator(container);

    // Grupo: Block elements
    addToggleButton(container, 'blockquote', 'quote', 'Cita', editor, 'blockquote',
        () => editor.chain().focus().toggleBlockquote().run());
    addToggleButton(container, 'codeblock', 'codeBlock', 'Bloque de código', editor, 'codeBlock',
        () => editor.chain().focus().toggleCodeBlock().run());
    addIconButton(container, 'hr', 'horizontalRule', 'Línea horizontal',
        () => editor.chain().focus().setHorizontalRule().run());
    addSeparator(container);

    // Grupo: Tablas / Link / Image
    addIconButton(container, 'table', 'table', 'Insertar tabla 3x3',
        () => editor.chain().focus().insertTable({ rows: 3, cols: 3, withHeaderRow: true }).run());
    addIconButton(container, 'link', 'link', 'Agregar link', () => {
        const url = window.prompt('URL del link:');
        if (url) editor.chain().focus().setLink({ href: url }).run();
    });
    addIconButton(container, 'image', 'image', 'Insertar imagen', () => {
        const url = window.prompt('URL de la imagen:');
        if (url) editor.chain().focus().setImage({ src: url }).run();
    });
    addSeparator(container);

    // Grupo: Paneles tipo Jira (5 botones)
    PANEL_TYPES.forEach(panel => {
        const btn = addIconButton(container, `panel-${panel.key}`, panel.icon,
            `Panel ${panel.label}`,
            () => editor.chain().focus().setPanel(panel.key).run());
        btn.classList.add(`da-toolbar-panel-${panel.key}`);
    });
}

function addIconButton(container, name, iconName, title, onClick) {
    const btn = document.createElement('button');
    btn.type = 'button';
    btn.className = `da-toolbar-btn da-toolbar-${name}`;
    btn.title = title;
    btn.innerHTML = iconHtml(iconName);
    btn.addEventListener('click', e => {
        e.preventDefault();
        onClick();
    });
    container.appendChild(btn);
    return btn;
}

function addToggleButton(container, name, iconName, title, editor, activeName, onClick) {
    const btn = addIconButton(container, name, iconName, title, onClick);
    const update = () => btn.classList.toggle('da-toolbar-active', editor.isActive(activeName));
    editor.on('selectionUpdate', update);
    editor.on('transaction', update);
    return btn;
}

function addSeparator(container) {
    const sep = document.createElement('div');
    sep.className = 'da-toolbar-separator';
    container.appendChild(sep);
}

function addHeadingDropdown(container, editor) {
    const select = document.createElement('select');
    select.className = 'da-toolbar-select';
    select.title = 'Estilo de párrafo';
    select.innerHTML = `
        <option value="paragraph">Texto normal</option>
        <option value="h1">Título 1</option>
        <option value="h2">Título 2</option>
        <option value="h3">Título 3</option>
        <option value="h4">Título 4</option>
    `;
    select.addEventListener('change', () => {
        const value = select.value;
        if (value === 'paragraph') {
            editor.chain().focus().setParagraph().run();
        } else {
            const level = parseInt(value.replace('h', ''), 10);
            editor.chain().focus().setHeading({ level }).run();
        }
    });

    const sync = () => {
        if (editor.isActive('heading', { level: 1 })) select.value = 'h1';
        else if (editor.isActive('heading', { level: 2 })) select.value = 'h2';
        else if (editor.isActive('heading', { level: 3 })) select.value = 'h3';
        else if (editor.isActive('heading', { level: 4 })) select.value = 'h4';
        else select.value = 'paragraph';
    };
    editor.on('selectionUpdate', sync);
    editor.on('transaction', sync);

    container.appendChild(select);
}

function addColorPicker(container, editor) {
    const wrapper = document.createElement('label');
    wrapper.className = 'da-toolbar-btn da-toolbar-color-picker';
    wrapper.title = 'Color de texto';
    wrapper.innerHTML = iconHtml('textColor');
    const input = document.createElement('input');
    input.type = 'color';
    input.value = '#172B4D';
    input.addEventListener('input', () => {
        editor.chain().focus().setColor(input.value).run();
    });
    wrapper.appendChild(input);
    container.appendChild(wrapper);
}

function addHighlightPicker(container, editor) {
    const wrapper = document.createElement('label');
    wrapper.className = 'da-toolbar-btn da-toolbar-highlight-picker';
    wrapper.title = 'Resaltado';
    wrapper.innerHTML = iconHtml('textBgColor');
    const input = document.createElement('input');
    input.type = 'color';
    input.value = '#FFEB3B';
    input.addEventListener('input', () => {
        editor.chain().focus().toggleHighlight({ color: input.value }).run();
    });
    wrapper.appendChild(input);
    container.appendChild(wrapper);
}
