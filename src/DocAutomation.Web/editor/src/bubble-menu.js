// Bubble menu contextual — aparece flotante sobre un panel cuando está seleccionado.
// Permite cambiar el tipo de panel y removerlo.

import { iconHtml } from './icons.js';

const PANEL_TYPES = [
    { key: 'info', label: 'Info' },
    { key: 'note', label: 'Note' },
    { key: 'success', label: 'Success' },
    { key: 'warning', label: 'Warning' },
    { key: 'error', label: 'Error' }
];

export function buildPanelBubbleMenu(container, editor) {
    container.innerHTML = '';
    container.classList.add('da-bubble-menu');

    PANEL_TYPES.forEach(panel => {
        const btn = document.createElement('button');
        btn.type = 'button';
        btn.className = `da-bubble-btn da-bubble-${panel.key}`;
        btn.title = `Cambiar a ${panel.label}`;
        btn.innerHTML = iconHtml(panel.key, 18);
        btn.addEventListener('click', e => {
            e.preventDefault();
            editor.chain().focus().updatePanelType(panel.key).run();
        });
        container.appendChild(btn);
    });

    const sep = document.createElement('div');
    sep.className = 'da-bubble-separator';
    container.appendChild(sep);

    const removeBtn = document.createElement('button');
    removeBtn.type = 'button';
    removeBtn.className = 'da-bubble-btn da-bubble-remove';
    removeBtn.title = 'Quitar panel';
    removeBtn.innerHTML = iconHtml('trash', 18);
    removeBtn.addEventListener('click', e => {
        e.preventDefault();
        editor.chain().focus().unsetPanel().run();
    });
    container.appendChild(removeBtn);
}
