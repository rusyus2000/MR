// src/composables/favorites.js
import { ref } from 'vue';
import { toggleFavoriteApi } from '../services/api';

export const favorites = ref(new Set()); // 

export async function toggleFavorite(id) {
    try {
        await toggleFavoriteApi(id);
        if (favorites.value.has(id)) {
            favorites.value.delete(id);
        } else {
            favorites.value.add(id);
        }
    } catch (err) {
        console.error('Toggle favorite failed', err);
    }
}

export function isFavorite(id) {
    return favorites.value.has(id);
}

//export function setFavorites(initial) {
//    if (!Array.isArray(initial)) {
//        console.warn('Invalid favorites list:', initial);
//        initial = [];
//    }
//    favorites.value = new Set(initial);
//}