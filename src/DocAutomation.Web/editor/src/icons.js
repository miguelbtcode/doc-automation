// Iconos del editor — todos los SVGs vienen del paquete @atlaskit/icon
// (Apache 2.0). Cada SVG se importa como string raw via el loader 'text' de
// esbuild y se inyecta como innerHTML en los botones.
//
// Los SVGs originales tienen viewBox 24x24 y usan fill="currentcolor", lo que
// significa que respetan el color del CSS del padre — ideal para temas
// claro/oscuro y para diferenciar visualmente los paneles.

// ===== Inline formatting =====
import bold from '@atlaskit/icon/svgs/editor/bold.svg';
import italic from '@atlaskit/icon/svgs/editor/italic.svg';
import underline from '@atlaskit/icon/svgs/editor/underline.svg';
import strike from '@atlaskit/icon/svgs/editor/strikethrough.svg';
import code from '@atlaskit/icon/svgs/editor/code.svg';
import textColor from '@atlaskit/icon/svgs/editor/text-color.svg';
import textBgColor from '@atlaskit/icon/svgs/editor/background-color.svg';
import removeFormatting from '@atlaskit/icon/svgs/editor/remove.svg';

// ===== Listas y bloques =====
import bulletList from '@atlaskit/icon/svgs/editor/bullet-list.svg';
import numberList from '@atlaskit/icon/svgs/editor/number-list.svg';
import quote from '@atlaskit/icon/svgs/editor/quote.svg';
import codeBlock from '@atlaskit/icon/svgs/editor/code.svg';
import horizontalRule from '@atlaskit/icon/svgs/editor/horizontal-rule.svg';

// ===== Alignment =====
import alignLeft from '@atlaskit/icon/svgs/editor/align-left.svg';
import alignCenter from '@atlaskit/icon/svgs/editor/align-center.svg';
import alignRight from '@atlaskit/icon/svgs/editor/align-right.svg';

// ===== Embeds =====
import table from '@atlaskit/icon/svgs/editor/table.svg';
import link from '@atlaskit/icon/svgs/editor/link.svg';
import image from '@atlaskit/icon/svgs/editor/image.svg';

// ===== Acciones =====
import undo from '@atlaskit/icon/svgs/editor/undo.svg';
import redo from '@atlaskit/icon/svgs/editor/redo.svg';
import trash from '@atlaskit/icon/svgs/trash.svg';

// ===== Paneles (los 5 tipos Jira) =====
import info from '@atlaskit/icon/svgs/editor/info.svg';
import note from '@atlaskit/icon/svgs/editor/note.svg';
import success from '@atlaskit/icon/svgs/editor/success.svg';
import warning from '@atlaskit/icon/svgs/editor/warning.svg';
import error from '@atlaskit/icon/svgs/editor/error.svg';
import panel from '@atlaskit/icon/svgs/editor/panel.svg';

// ===== Headings =====
import textStyle from '@atlaskit/icon/svgs/editor/text-style.svg';

export const icons = {
    // formatting
    bold,
    italic,
    underline,
    strike,
    code,
    textColor,
    textBgColor,
    removeFormatting,
    // bloques
    bulletList,
    numberList,
    quote,
    codeBlock,
    horizontalRule,
    // alignment
    alignLeft,
    alignCenter,
    alignRight,
    // embeds
    table,
    link,
    image,
    // acciones
    undo,
    redo,
    trash,
    // paneles
    info,
    note,
    success,
    warning,
    error,
    panel,
    // headings
    textStyle
};

/**
 * Crea un wrapper <span> con el SVG inline. Devuelve un string HTML.
 * Útil para usar en innerHTML de los botones del toolbar.
 */
export function iconHtml(name, size = 20) {
    const svg = icons[name];
    if (!svg) {
        console.warn(`[icons] Icon not found: ${name}`);
        return '';
    }
    // Reemplazamos el width/height del SVG original (24x24) con el size pedido
    return svg.replace(/width="\d+"/, `width="${size}"`).replace(/height="\d+"/, `height="${size}"`);
}
