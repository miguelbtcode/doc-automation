// Bubble menu para panels — aparece cuando seleccionás un panel

import { iconHtml } from './icons.js';

const PANEL_TYPES = [
    { key: 'info',    matIcon: 'info',         label: 'Info',    color: '#0747A6' },
    { key: 'note',    matIcon: 'description',   label: 'Note',    color: '#403294' },
    { key: 'success', matIcon: 'check_circle',  label: 'Tip',     color: '#006644' },
    { key: 'warning', matIcon: 'warning',        label: 'Warning', color: '#FF8B00' },
    { key: 'error',   matIcon: 'cancel',         label: 'Error',   color: '#BF2600' }
];

export function buildPanelBubbleMenu(container, editor) {
    container.innerHTML = '';
    container.classList.add('da-bubble-menu', 'da-panel-bubble-menu');

    const dropdownWrapper = document.createElement('div');
    dropdownWrapper.className = 'da-bubble-dropdown-wrapper';

    const dropdownBtn = document.createElement('button');
    dropdownBtn.type = 'button';
    dropdownBtn.className = 'da-bubble-dropdown-btn da-panel-type-btn';
    dropdownBtn.title = 'Cambiar tipo de panel';
    dropdownBtn.innerHTML = `<span class="da-panel-type-icon"><span class="material-icons" style="font-size:18px;line-height:1">info</span></span>`;

    const dropdownMenu = document.createElement('div');
    dropdownMenu.className = 'da-bubble-dropdown-menu';

    PANEL_TYPES.forEach(panel => {
        const item = document.createElement('button');
        item.type = 'button';
        item.className = `da-bubble-dropdown-item da-panel-dropdown-item`;
        item.dataset.panelType = panel.key;
        item.innerHTML = `
            <span class="da-bubble-dropdown-icon" style="color: ${panel.color}">${panel.matIcon ? `<span class="material-icons" style="font-size:16px;line-height:1">${panel.matIcon}</span>` : ''}</span>
            <span class="da-panel-item-label">${panel.label}</span>
        `;
        item.addEventListener('click', e => {
            e.preventDefault();
            e.stopPropagation();
            editor.chain().focus().updatePanelType(panel.key).run();
            dropdownMenu.classList.remove('da-bubble-dropdown-open');
        });
        dropdownMenu.appendChild(item);
    });

    dropdownBtn.addEventListener('click', e => {
        e.preventDefault();
        e.stopPropagation();
        dropdownMenu.classList.toggle('da-bubble-dropdown-open');
        updateIcon(editor);
    });

    document.addEventListener('click', () => {
        dropdownMenu.classList.remove('da-bubble-dropdown-open');
    });

    dropdownWrapper.appendChild(dropdownBtn);
    dropdownWrapper.appendChild(dropdownMenu);
    container.appendChild(dropdownWrapper);

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

    function updateIcon(editorInstance) {
        const currentType = editorInstance.getAttributes('panel').panelType || 'info';
        const panelInfo = PANEL_TYPES.find(p => p.key === currentType) || PANEL_TYPES[0];
        const iconSpan = dropdownBtn.querySelector('.da-panel-type-icon');
        const iconName = panelInfo.matIcon ?? 'edit_note';
        iconSpan.innerHTML = `<span class="material-icons" style="font-size:18px;line-height:1">${iconName}</span>`;
        iconSpan.style.color = panelInfo.color;
    }

    editor.on('selectionUpdate', () => updateIcon(editor));
    editor.on('transaction', () => updateIcon(editor));
}
