// UX Polish: theme toggle, keyboard shortcuts, unsaved changes guard, recent actions persistence
window.uxPolish = (function () {
    const THEME_KEY = 'doc-automation.theme';
    const THEME_LINK_ID = 'radzen-theme';
    const RECENT_ACTIONS_KEY = 'doc-automation.recent-actions';
    const DEFAULT_THEME = 'standard-base';

    let hasUnsavedChanges = false;
    let keyboardHandler = null;
    let keyListenerAttached = false;

    function setThemeLink(theme) {
        const link = document.getElementById(THEME_LINK_ID);
        if (link) {
            link.href = `_content/Radzen.Blazor/css/${theme}.css`;
        }
        document.documentElement.setAttribute('data-theme', theme.includes('dark') ? 'dark' : 'light');
    }

    return {
        // ===== THEME =====
        getTheme: function () {
            return localStorage.getItem(THEME_KEY) || DEFAULT_THEME;
        },
        setTheme: function (theme) {
            localStorage.setItem(THEME_KEY, theme);
            setThemeLink(theme);
        },
        initTheme: function () {
            const theme = this.getTheme();
            setThemeLink(theme);
            return theme;
        },

        // ===== KEYBOARD SHORTCUTS =====
        registerKeyboard: function (dotNetRef) {
            keyboardHandler = dotNetRef;
            if (!keyListenerAttached) {
                document.addEventListener('keydown', function (e) {
                    if (!keyboardHandler) return;

                    // Ignore shortcuts inside editable fields except when it's our modifier combo
                    const target = e.target;
                    const isInput = target && (
                        target.tagName === 'INPUT' ||
                        target.tagName === 'TEXTAREA' ||
                        target.isContentEditable
                    );

                    const ctrlMeta = e.ctrlKey || e.metaKey;
                    let key = null;

                    if (ctrlMeta && e.key.toLowerCase() === 's') {
                        key = 'ctrl+s';
                    } else if (ctrlMeta && e.key.toLowerCase() === 'k') {
                        key = 'ctrl+k';
                    } else if (e.key === 'Escape' && !isInput) {
                        key = 'escape';
                    }

                    if (key) {
                        e.preventDefault();
                        keyboardHandler.invokeMethodAsync('OnKeyboardShortcut', key);
                    }
                });
                keyListenerAttached = true;
            }
        },
        unregisterKeyboard: function () {
            keyboardHandler = null;
        },

        // ===== UNSAVED CHANGES GUARD =====
        setUnsavedChanges: function (value) {
            hasUnsavedChanges = !!value;
        },
        initBeforeUnload: function () {
            window.addEventListener('beforeunload', function (e) {
                if (hasUnsavedChanges) {
                    e.preventDefault();
                    e.returnValue = '';
                    return '';
                }
            });
        },

        // ===== RECENT ACTIONS (persistent log) =====
        loadRecentActions: function () {
            try {
                const raw = localStorage.getItem(RECENT_ACTIONS_KEY);
                return raw ? JSON.parse(raw) : [];
            } catch {
                return [];
            }
        },
        saveRecentActions: function (actions) {
            try {
                localStorage.setItem(RECENT_ACTIONS_KEY, JSON.stringify(actions));
            } catch {
                // localStorage full or disabled — silently ignore
            }
        },
        clearRecentActions: function () {
            localStorage.removeItem(RECENT_ACTIONS_KEY);
        }
    };
})();
