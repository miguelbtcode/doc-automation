// Panel Node — Tiptap node para paneles tipo Jira/Confluence
// Renderiza: <div data-panel-type="..."><div class="da-panel-content">...</div></div>
// Compatible con Jira Cloud al copiar pegar

import { Node, mergeAttributes, wrappingInputRule } from '@tiptap/core';

export const PANEL_TYPES = ['info', 'note', 'success', 'warning', 'error'];

export const PanelNode = Node.create({
    name: 'panel',
    group: 'block',
    content: 'block+',
    defining: true,

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
