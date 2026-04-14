// Panel Node — Tiptap node para paneles tipo Jira/Confluence
//
// Comportamiento:
//   - Click en icono → dropdown con los 5 tipos, click en tipo → cambia el panel
//   - Click en drag handle → ProseMirror gestiona drag & drop
//   - Click en borde/padding del panel → selecciona el nodo como componente
//   - Click en contenido → edición normal

import { Node, mergeAttributes, wrappingInputRule } from '@tiptap/core';

export const PANEL_TYPES = ['info', 'note', 'success', 'warning', 'error'];

export const PANEL_META = {
    info:    { icon: 'info',         color: '#0747A6', label: 'Info' },
    note:    { icon: 'description',  color: '#403294', label: 'Note' },
    success: { icon: 'check_circle', color: '#006644', label: 'Tip' },
    warning: { icon: 'warning',      color: '#FF8B00', label: 'Warning' },
    error:   { icon: 'cancel',       color: '#BF2600', label: 'Error' },
};

export const PanelNode = Node.create({
    name: 'panel',
    group: 'block',
    content: 'block+',
    defining: true,
    draggable: true,

    addAttributes() {
        return {
            panelType: {
                default: 'info',
                parseHTML: element => element.getAttribute('data-panel-type') || 'info',
                renderHTML: attributes => ({ 'data-panel-type': attributes.panelType })
            }
        };
    },

    parseHTML() {
        return [{
            tag: 'div[data-panel-type]',
            getAttrs: element => {
                const type = element.getAttribute('data-panel-type');
                return PANEL_TYPES.includes(type) ? { panelType: type } : false;
            }
        }];
    },

    renderHTML({ HTMLAttributes }) {
        const type = HTMLAttributes['data-panel-type'] || 'info';
        return [
            'div',
            mergeAttributes(HTMLAttributes, { class: `da-panel da-panel-${type}` }),
            ['div', { class: 'da-panel-content' }, 0]
        ];
    },

    addNodeView() {
        return ({ node, getPos, editor }) => {
            let currentType = node.attrs.panelType || 'info';

            // ── Wrapper (dom raíz del NodeView) ──────────────────────────────
            const wrapper = document.createElement('div');
            wrapper.className = 'da-panel-wrapper';

            // ── Drag handle (fuera del box) ───────────────────────────────────
            // draggable=true es OBLIGATORIO: HTML5 D&D no sube por el árbol buscando
            // un ancestro draggable, el elemento sobre el que se hace mousedown debe serlo.
            const dragEl = document.createElement('span');
            dragEl.className = 'da-panel-drag material-icons';
            dragEl.contentEditable = 'false';
            dragEl.draggable = true;
            dragEl.textContent = 'drag_indicator';
            dragEl.title = 'Arrastrar';

            // ── Panel box ─────────────────────────────────────────────────────
            const panelEl = document.createElement('div');
            panelEl.className = `da-panel da-panel-${currentType}`;
            panelEl.setAttribute('data-panel-type', currentType);

            // Icono del tipo — click abre el selector de tipo
            const iconEl = document.createElement('span');
            iconEl.className = 'da-panel-icon material-icons';
            iconEl.contentEditable = 'false';
            iconEl.title = 'Cambiar tipo';

            // Área editable
            const contentEl = document.createElement('div');
            contentEl.className = 'da-panel-content';

            panelEl.appendChild(iconEl);
            panelEl.appendChild(contentEl);
            wrapper.appendChild(dragEl);
            wrapper.appendChild(panelEl);

            // ── Helpers ───────────────────────────────────────────────────────
            function syncIcon(type) {
                const m = PANEL_META[type] ?? PANEL_META.info;
                iconEl.textContent = m.icon;
                iconEl.style.color = m.color;
            }

            // ── Eventos ───────────────────────────────────────────────────────

            // Borde / padding del panel (no icono, no contenido) → seleccionar nodo.
            // El toggle del picker lo maneja editor-factory.js en capture-phase.
            wrapper.addEventListener('mousedown', (e) => {
                if (contentEl.contains(e.target)) return;
                if (iconEl.contains(e.target)) return;
                if (dragEl.contains(e.target)) return;
                e.preventDefault();
                const pos = getPos();
                if (typeof pos === 'number') editor.commands.setNodeSelection(pos);
            });

            // ── Init ──────────────────────────────────────────────────────────
            syncIcon(currentType);

            return {
                dom: wrapper,
                contentDOM: contentEl,

                stopEvent(event) {
                    if (contentEl.contains(event.target)) return false;
                    if (dragEl.contains(event.target)) return false;
                    return true;
                },

                update(updatedNode) {
                    if (updatedNode.type.name !== 'panel') return false;
                    currentType = updatedNode.attrs.panelType || 'info';
                    panelEl.className = `da-panel da-panel-${currentType}`;
                    panelEl.setAttribute('data-panel-type', currentType);
                    syncIcon(currentType);
                    return true;
                },

                destroy() {}
            };
        };
    },

    addCommands() {
        return {
            setPanel: (panelType = 'info') => ({ commands }) =>
                commands.wrapIn(this.name, { panelType }),
            togglePanel: (panelType = 'info') => ({ commands }) =>
                commands.toggleWrap(this.name, { panelType }),
            unsetPanel: () => ({ commands }) =>
                commands.lift(this.name),
            updatePanelType: (panelType) => ({ commands }) =>
                commands.updateAttributes(this.name, { panelType })
        };
    },

    addInputRules() {
        return PANEL_TYPES.map(type =>
            wrappingInputRule({
                find: new RegExp(`^:${type}\\s$`),
                type: this.type,
                getAttributes: () => ({ panelType: type })
            })
        );
    },

    addKeyboardShortcuts() {
        return { 'Mod-Enter': () => this.editor.commands.exitCode() };
    }
});
