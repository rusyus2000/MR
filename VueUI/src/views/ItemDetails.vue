<template>
    <div class="modal-backdrop">
        <div class="modal-content shadow">
            <div class="modal-header">
                <h5 class="modal-title">
                    <i v-if="item && item.featured" class="bi bi-star-fill featured-icon me-1" title="Featured"></i>
                    {{ item.title }}
                </h5>
                <button class="btn-close" @click="$emit('close')"></button>
            </div>
            <div class="modal-body">
                <p><strong>Description:</strong> {{ item.description }}</p>
                <p><strong>URL:</strong> <a href="#" @click.prevent="openResource(item)">{{ item.url }}</a></p>
                <p><strong>Asset Type:</strong> {{ item.assetTypeName }}</p>
                <p><strong>Domain:</strong> {{ item.domain }}</p>
                <p><strong>Division:</strong> {{ item.division }}</p>
                <p><strong>Service Line:</strong> {{ item.serviceLine }}</p>
                <p><strong>Data Source:</strong> {{ item.dataSource }}</p>
                <p><strong>Contains PHI:</strong> {{ item.privacyPhi ? 'Yes' : 'No' }}</p>
                <p><strong>Date Added:</strong> {{ new Date(item.dateAdded).toLocaleDateString() }}</p>
            </div>
        </div>
    </div>
</template>

<script>
    export default {
        name: 'ModalAssetDetails',
        props: {
            item: Object
        }
        ,
        methods: {
            async openResource(item) {
                try {
                    const api = await import('../services/api');
                    await api.recordOpen(item.id);
                } catch (err) {
                    console.error('record open failed', err);
                }
                window.open(item.url, '_blank', 'noopener');
            }
        }
    };
</script>

<style scoped>
    .modal-backdrop {
        position: fixed;
        top: 0;
        left: 0;
        width: 100vw;
        height: 100vh;
        background-color: rgba(0,0,0,0.5);
        display: flex;
        justify-content: center;
        align-items: center;
        z-index: 1050;
    }

    .modal-content {
        background-color: white;
        border-radius: 8px;
        padding: 1.5rem;
        max-width: 600px;
        width: 100%;
    }

    .modal-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 1rem;
    }

    .featured-icon {
        color: #FFD700;
        font-size: 0.95rem;
        vertical-align: text-bottom;
    }
</style>
