<!-- src/components/ModalAddAsset.vue -->
<template>
    <div class="modal-backdrop">
        <div class="modal-panel">
            <h5 class="mb-3">Add New Asset</h5>
            <form @submit.prevent="submitForm">
                <div class="mb-2">
                    <label class="form-label">Title</label>
                    <input v-model="form.title" type="text" class="form-control" required />
                </div>

                <div class="mb-2">
                    <label class="form-label">Description</label>
                    <textarea v-model="form.description" class="form-control" required></textarea>
                </div>

                <div class="mb-2">
                    <label class="form-label">URL</label>
                    <input v-model="form.url" type="url" class="form-control" required />
                </div>

                <div class="mb-2">
                    <label class="form-label">Asset Types</label>
                    <select v-model="form.assetTypes" multiple class="form-select" required>
                        <option v-for="opt in lookup.assetTypes" :key="opt.value" :value="opt.value">
                            {{ opt.value }}
                        </option>
                    </select>
                </div>

                <div class="row">
                    <div class="col mb-2">
                        <label class="form-label">Domain</label>
                        <select v-model="form.domain" class="form-select" required>
                            <option v-for="opt in lookup.domains" :key="opt.value" :value="opt.value">
                                {{ opt.value }}
                            </option>
                        </select>
                    </div>
                    <div class="col mb-2">
                        <label class="form-label">Division</label>
                        <select v-model="form.division" class="form-select" required>
                            <option v-for="opt in lookup.divisions" :key="opt.value" :value="opt.value">
                                {{ opt.value }}
                            </option>
                        </select>
                    </div>
                </div>

                <div class="row">
                    <div class="col mb-2">
                        <label class="form-label">Service Line</label>
                        <select v-model="form.serviceLine" class="form-select" required>
                            <option v-for="opt in lookup.serviceLines" :key="opt.value" :value="opt.value">
                                {{ opt.value }}
                            </option>
                        </select>
                    </div>
                    <div class="col mb-2">
                        <label class="form-label">Data Source</label>
                        <select v-model="form.dataSource" class="form-select" required>
                            <option v-for="opt in lookup.dataSources" :key="opt.value" :value="opt.value">
                                {{ opt.value }}
                            </option>
                        </select>
                    </div>
                </div>

                <div class="form-check mb-3">
                    <input v-model="form.privacyPhi" class="form-check-input" type="checkbox" id="privacyPhi" />
                    <label class="form-check-label" for="privacyPhi">Contains PHI</label>
                </div>

                <div class="d-flex justify-content-between">
                    <button type="button" class="btn btn-outline-secondary" @click="$emit('close')">Cancel</button>
                    <button type="submit" class="btn btn-success" :disabled="saving">
                        <span v-if="saving" class="spinner-border spinner-border-sm me-2" />
                        Save
                    </button>
                </div>
            </form>
        </div>
    </div>
</template>

<script setup>
    import { ref, onMounted } from 'vue'
    import { fetchLookup, createItem } from '../services/api'

    const emit = defineEmits(['close', 'saved'])

    const saving = ref(false)

    const form = ref({
        title: '',
        description: '',
        url: '',
        assetTypes: [],
        domain: '',
        division: '',
        serviceLine: '',
        dataSource: '',
        privacyPhi: false,
    })

    const lookup = ref({
        domains: [],
        divisions: [],
        serviceLines: [],
        dataSources: [],
        assetTypes: [],
    })

    onMounted(async () => {
        lookup.value.domains = await fetchLookup('Domain')
        lookup.value.divisions = await fetchLookup('Division')
        lookup.value.serviceLines = await fetchLookup('ServiceLine')
        lookup.value.dataSources = await fetchLookup('DataSource')
        lookup.value.assetTypes = await fetchLookup('AssetType')
    })

    async function submitForm() {
        saving.value = true
        try {
            await createItem(form.value)
            emit('saved') // will call ItemGrid.handleAssetSaved
        } catch (err) {
            alert('Error saving asset: ' + err.message)
        } finally {
            saving.value = false
        }
    }
</script>

<style scoped>
    .modal-backdrop {
        position: fixed;
        inset: 0;
        background-color: rgba(0, 0, 0, 0.4);
        display: flex;
        align-items: center;
        justify-content: center;
        z-index: 1000;
    }

    .modal-panel {
        background: white;
        padding: 2rem;
        width: 650px;
        border-radius: 8px;
        box-shadow: 0 15px 40px rgba(0, 0, 0, 0.2);
    }
</style>
