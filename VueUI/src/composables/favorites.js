// favorites.js
import { ref, watch } from 'vue';

const stored = JSON.parse(localStorage.getItem('favorites') || '[]');
export const favorites = ref(new Set(stored));

// Persist to localStorage when updated
watch(favorites, (newVal) => {
    localStorage.setItem('favorites', JSON.stringify(Array.from(newVal)));
}, { deep: true });

export function toggleFavorite(id) {
    if (favorites.value.has(id)) {
        favorites.value.delete(id);
    } else {
        favorites.value.add(id);
    }
}

export function isFavorite(id) {
    return favorites.value.has(id);
}
