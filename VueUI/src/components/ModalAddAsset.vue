<template>
    <div class="modal-backdrop">
        <div class="modal-content shadow">
            <div class="modal-header custom-header">
                <h5 class="modal-title">Add New Asset</h5>
                <button class="btn-close" @click="$emit('close')"></button>
            </div>
            <div class="modal-body">
                <form @submit.prevent="submitForm">
                    <div class="details-grid">
                        <div class="label">Title:</div>
                        <div><input v-model="form.title" type="text" class="form-control" required /></div>

                        <div class="label">Description:</div>
                        <div><textarea v-model="form.description" class="form-control" required /></div>

                        <div class="label">URL:</div>
                        <div><input v-model="form.url" type="url" class="form-control" required /></div>

                        <div class="label">Asset Types:</div>
                        <div>
                            <select v-model="form.assetTypes" multiple class="form-select" required>
                                <option v-for="opt in lookup.assetTypes" :key="opt.value" :value="opt.value">
                                    {{ opt.value }}
                                </option>
                            </select>
                        </div>

                        <div class="label">Domain:</div>
                        <div>
                            <select v-model="form.domain" class="form-select" required>
                                <option v-for="opt in lookup.domains" :key="opt.value" :value="opt.value">
                                    {{ opt.value }}
                                </option>
                            </select>
                        </div>

                        <div class="label">Division:</div>
                        <div>
                            <select v-model="form.division" class="form-select" required>
                                <option v-for="opt in lookup.divisions" :key="opt.value" :value="opt.value">
                                    {{ opt.value }}
                                </option>
                            </select>
                        </div>

                        <div class="label">Service Line:</div>
                        <div>
                            <select v-model="form.serviceLine" class="form-select" required>
                                <option v-for="opt in lookup.serviceLines" :key="opt.value" :value="opt.value">
                                    {{ opt.value }}
                                </option>
                            </select>
                        </div>

                        <div class="label">Data Source:</div>
                        <div>
                            <select v-model="form.dataSource" class="form-select" required>
                                <option v-for="opt in lookup.dataSources" :key="opt.value" :value="opt.value">
                                    {{ opt.value }}
                                </option>
                            </select>
                        </div>

                        <div class="label">Contains PHI:</div>
                        <div>
                            <input v-model="form.privacyPhi" class="form-check-input" type="checkbox" id="privacyPhi" />
                        </div>
                    </div>

                    <div class="d-flex justify-content-between mt-4">
                        <button type="button" class="btn btn-outline-secondary" @click="$emit('close')">Cancel</button>
                        <button type="submit" class="btn btn-success" :disabled="saving">
                            <span v-if="saving" class="spinner-border spinner-border-sm me-2" />
                            Save
                        </button>
                    </div>
                </form>
            </div>
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

    .modal-content {
        background: white;
        padding: 0;
        width: 700px;
        border-radius: 8px;
        box-shadow: 0 15px 40px rgba(0, 0, 0, 0.2);
    }

    .modal-header.custom-header {
        background-color: #f0f4f8;
        padding: 1rem 1.5rem;
        border-bottom: 1px solid #dee2e6;
        display: flex;
        justify-content: space-between;
        align-items: center;
        border-top-left-radius: 8px;
        border-top-right-radius: 8px;
    }

    .modal-body {
        padding: 1.5rem;
    }

    .details-grid {
        display: grid;
        grid-template-columns: max-content 1fr;
        row-gap: 0.75rem;
        column-gap: 1rem;
        align-items: center;
    }

    .label {
        font-weight: 600;
        white-space: nowrap;
    }
</style>
