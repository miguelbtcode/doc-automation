// Custom Panel Node — Tiptap node que renderiza paneles tipo Jira/Confluence.
//
// Cada panel se renderiza como <div data-panel-type="info|note|success|warning|error">.
// Cuando este HTML se pega en el editor de Jira/Confluence Cloud, su parser ProseMirror
// reconoce el atributo `data-panel-type` y lo convierte automáticamente a un panel nativo.
//
// Internamente es un Node con `content: 'block+'`, lo que significa que puede contener
// cualquier bloque (párrafos, listas, headings, etc.) — igual que los panels reales de Jira.

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
        return [
            {
                tag: 'div[data-panel-type]',
                getAttrs: element => {
                    const type = element.getAttribute('data-panel-type');
                    return PANEL_TYPES.includes(type) ? { panelType: type } : false;
                }
            }
        ];
    },

    renderHTML({ HTMLAttributes }) {
        const type = HTMLAttributes['data-panel-type'] || 'info';
        // Estructura simple: <div data-panel-type><div class="da-panel-content">...</div></div>
        // El ícono lo pone el CSS via ::before con un background SVG inline.
        // Esto mantiene el HTML serializado limpio y compatible con Jira al pegar.
        return [
            'div',
            mergeAttributes(HTMLAttributes, {
                class: `da-panel da-panel-${type}`
            }),
            ['div', { class: 'da-panel-content' }, 0]
        ];
    },

    addCommands() {
        return {
            setPanel: (panelType = 'info') => ({ commands }) => {
                return commands.wrapIn(this.name, { panelType });
            },
            togglePanel: (panelType = 'info') => ({ commands }) => {
                return commands.toggleWrap(this.name, { panelType });
            },
            unsetPanel: () => ({ commands }) => {
                return commands.lift(this.name);
            },
            updatePanelType: (panelType) => ({ commands }) => {
                return commands.updateAttributes(this.name, { panelType });
            }
        };
    },

    addInputRules() {
        // Permite escribir ":info" + Space al inicio de un párrafo vacío para crear un panel
        return PANEL_TYPES.map(type =>
            wrappingInputRule({
                find: new RegExp(`^:${type}\\s$`),
                type: this.type,
                getAttributes: () => ({ panelType: type })
            })
        );
    },

    addKeyboardShortcuts() {
        return {
            // Escape "saca" al usuario del panel insertando un párrafo después
            'Mod-Enter': () => this.editor.commands.exitCode()
        };
    }
});

