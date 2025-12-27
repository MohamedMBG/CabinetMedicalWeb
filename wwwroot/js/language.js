(function () {
    const IGNORED_TAGS = new Set(["SCRIPT", "STYLE", "NOSCRIPT", "CODE", "PRE", "OPTION", "TEXTAREA"]);
    const TRANSLATABLE_ATTRIBUTES = ["placeholder", "aria-label", "title", "alt", "value"];
    const STORAGE_KEY = 'preferredLanguage';
    const FALLBACK_LANGUAGE = 'en';

    const state = {
        textNodes: [],
        attributeEntries: [],
        textOriginals: new WeakMap(),
        attributeOriginals: new WeakMap(),
        trigger: null,
        modalOverlay: null,
        modal: null,
        languageButtons: [],
        currentLanguage: FALLBACK_LANGUAGE
    };

    function collectTextNodes() {
        const walker = document.createTreeWalker(document.body, NodeFilter.SHOW_TEXT, {
            acceptNode(node) {
                if (!node.parentElement || IGNORED_TAGS.has(node.parentElement.tagName)) {
                    return NodeFilter.FILTER_REJECT;
                }

                const text = node.textContent?.trim();
                if (!text || text.length < 2) {
                    return NodeFilter.FILTER_REJECT;
                }

                return NodeFilter.FILTER_ACCEPT;
            }
        });

        let current;
        while ((current = walker.nextNode())) {
            state.textNodes.push(current);
            state.textOriginals.set(current, current.textContent);
        }
    }

    function collectAttributeNodes() {
        const elements = document.querySelectorAll('*');
        elements.forEach(element => {
            TRANSLATABLE_ATTRIBUTES.forEach(attribute => {
                const value = element.getAttribute(attribute);
                if (value && value.trim().length > 1) {
                    state.attributeEntries.push({ element, attribute });
                    if (!state.attributeOriginals.has(element)) {
                        state.attributeOriginals.set(element, {});
                    }
                    state.attributeOriginals.get(element)[attribute] = value;
                }
            });
        });
    }

    function restoreOriginals() {
        state.textNodes.forEach(node => {
            const original = state.textOriginals.get(node);
            if (typeof original === 'string') {
                node.textContent = original;
            }
        });

        state.attributeEntries.forEach(({ element, attribute }) => {
            const originals = state.attributeOriginals.get(element) || {};
            if (originals[attribute]) {
                element.setAttribute(attribute, originals[attribute]);
            }
        });
    }

    async function requestTranslations(texts, lang) {
        const translations = [];
        const chunks = [];

        for (let i = 0; i < texts.length; i += 50) {
            chunks.push(texts.slice(i, i + 50));
        }

        for (const chunk of chunks) {
            try {
                const response = await fetch('/api/translate/batch', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ language: lang, texts: chunk })
                });

                if (!response.ok) {
                    console.error('Translation failed', await response.text());
                    translations.push(...chunk);
                    continue;
                }

                const data = await response.json();
                translations.push(...(data.translations || chunk));
            } catch (error) {
                console.error('Translation error', error);
                translations.push(...chunk);
            }
        }

        return translations;
    }

    async function translatePage(lang) {
        if (lang === FALLBACK_LANGUAGE) {
            restoreOriginals();
            updateLanguage(lang);
            return;
        }

        const textValues = state.textNodes
            .map(node => state.textOriginals.get(node))
            .filter(value => typeof value === 'string' && value.trim().length > 0);

        const attributeValues = state.attributeEntries
            .map(({ element, attribute }) => (state.attributeOriginals.get(element) || {})[attribute])
            .filter(value => typeof value === 'string' && value.trim().length > 0);

        const uniqueTexts = [...new Set([...textValues, ...attributeValues])];

        if (uniqueTexts.length === 0) {
            updateLanguage(lang);
            return;
        }

        const translations = await requestTranslations(uniqueTexts, lang);
        const translationMap = new Map();

        uniqueTexts.forEach((text, index) => {
            translationMap.set(text, translations[index] || text);
        });

        state.textNodes.forEach(node => {
            const original = state.textOriginals.get(node);
            if (translationMap.has(original)) {
                node.textContent = translationMap.get(original);
            }
        });

        state.attributeEntries.forEach(({ element, attribute }) => {
            const original = (state.attributeOriginals.get(element) || {})[attribute];
            if (translationMap.has(original)) {
                element.setAttribute(attribute, translationMap.get(original));
            }
        });

        updateLanguage(lang);
    }

    function updateLanguage(lang) {
        state.currentLanguage = lang;
        document.documentElement.setAttribute('lang', lang);
        localStorage.setItem(STORAGE_KEY, lang);
        if (state.trigger) {
            const label = document.getElementById('languageTriggerLabel');
            if (label) {
                const text = lang === 'fr' ? 'Français' : lang === 'ar' ? 'العربية' : 'English';
                label.textContent = text;
            }
            state.trigger.setAttribute('aria-expanded', 'false');
        }
    }

    function closeModal() {
        if (!state.modalOverlay) return;
        state.modalOverlay.classList.remove('show');
        state.modalOverlay.setAttribute('hidden', '');
        document.body.classList.remove('language-modal-open');
        if (state.trigger) {
            state.trigger.focus();
            state.trigger.setAttribute('aria-expanded', 'false');
        }
    }

    async function handleLanguageSelection(lang) {
        await translatePage(lang);
        closeModal();
    }

    function openModal() {
        if (!state.modalOverlay || !state.modal) return;
        state.modalOverlay.classList.add('show');
        state.modalOverlay.removeAttribute('hidden');
        document.body.classList.add('language-modal-open');
        const firstButton = state.modal.querySelector('.language-option');
        if (firstButton) {
            firstButton.focus();
        }
        if (state.trigger) {
            state.trigger.setAttribute('aria-expanded', 'true');
        }
    }

    function initializeUI() {
        state.trigger = document.getElementById('languageTrigger');
        state.modalOverlay = document.getElementById('languageModalOverlay');
        state.modal = document.getElementById('languageModal');
        state.languageButtons = Array.from(document.querySelectorAll('.language-option'));

        if (state.trigger) {
            state.trigger.addEventListener('click', openModal);
        }

        state.languageButtons.forEach(button => {
            button.addEventListener('click', () => handleLanguageSelection(button.dataset.lang));
            button.addEventListener('keydown', (event) => {
                if (event.key === 'Enter' || event.key === ' ') {
                    event.preventDefault();
                    handleLanguageSelection(button.dataset.lang);
                }
            });
        });

        if (state.modalOverlay) {
            state.modalOverlay.addEventListener('click', (event) => {
                if (event.target === state.modalOverlay) {
                    event.stopPropagation();
                }
            });
        }
    }

    async function initializeLanguage() {
        collectTextNodes();
        collectAttributeNodes();

        const savedLanguage = localStorage.getItem(STORAGE_KEY);

        if (savedLanguage) {
            updateLanguage(savedLanguage);
            if (savedLanguage !== FALLBACK_LANGUAGE) {
                await translatePage(savedLanguage);
            }
            return;
        }

        openModal();
        document.addEventListener('keydown', (event) => {
            if (event.key === 'Escape' && state.modalOverlay?.classList.contains('show')) {
                event.preventDefault();
            }
        });
    }

    document.addEventListener('DOMContentLoaded', () => {
        initializeUI();
        initializeLanguage();
    });
})();
