<template>
    <div>
        <Navbar />

        <div class="container my-5" style="max-width: 60%;">
            <div class="card shadow-custom mb-5">
                <div class="card-body">
                    <h3 class="card-title mb-4">Add New Asset</h3>

                    <form @submit.prevent="submitForm">
                        <!-- First three full-width fields -->
                        <div class="mb-3">
                            <label class="form-label fw-bold">Title</label>
                            <input v-model="form.title" type="text" class="form-control" required />
                        </div>

                        <div class="mb-3">
                            <label class="form-label fw-bold">Description</label>
                            <textarea v-model="form.description" class="form-control" required></textarea>
                        </div>

                        <div class="mb-3">
                            <label class="form-label fw-bold">URL</label>
                            <input v-model="form.url" type="url" class="form-control" required />
                        </div>

                        <!-- Two-column grid for the rest -->
                        <div class="row row-custom mb-4">
                            <div class="col asset-span-2 mb-3">
                                <label class="form-label fw-bold">Asset Type</label>
                                <select v-model.number="form.assetTypeId"
                                        class="form-select w-100"
                                        required>
                                    <option v-for="opt in lookup.assetTypes" :key="opt.id" :value="opt.id">
                                        {{ opt.value }}
                                    </option>
                                </select>
                            </div>

                            <div class="col mb-3">
                                <label class="form-label fw-bold">Domain</label>
                                <select v-model.number="form.domainId" @change="onLookupChange('domains','domainId','domain')" class="form-select w-100" required>
                                    <option v-for="opt in lookup.domains" :key="opt.id" :value="opt.id">
                                        {{ opt.value }}
                                    </option>
                                </select>
                            </div>

                            <div class="col mb-3">
                                <label class="form-label fw-bold">Division</label>
                                <select v-model.number="form.divisionId" @change="onLookupChange('divisions','divisionId','division')" class="form-select w-100" required>
                                    <option v-for="opt in lookup.divisions" :key="opt.id" :value="opt.id">
                                        {{ opt.value }}
                                    </option>
                                </select>
                            </div>

                            <div class="col mb-3">
                                <label class="form-label fw-bold">Service Line</label>
                                <select v-model.number="form.serviceLineId" @change="onLookupChange('serviceLines','serviceLineId','serviceLine')" class="form-select w-100" required>
                                    <option v-for="opt in lookup.serviceLines" :key="opt.id" :value="opt.id">
                                        {{ opt.value }}
                                    </option>
                                </select>
                            </div>

                            <div class="col mb-3">
                                <label class="form-label fw-bold">Data Source</label>
                                <select v-model.number="form.dataSourceId" @change="onLookupChange('dataSources','dataSourceId','dataSource')" class="form-select w-100" required>
                                    <option v-for="opt in lookup.dataSources" :key="opt.id" :value="opt.id">
                                        {{ opt.value }}
                                    </option>
                                </select>
                            </div>

                            <div class="col mb-3">
                                <label class="form-label fw-bold">Privacy (PHI)</label>
                                <div class="form-check">
                                    <input v-model="form.privacyPhi"
                                           class="form-check-input"
                                           type="checkbox"
                                           id="privacyPhi" />
                                    <label class="form-check-label" for="privacyPhi">Contains PHI</label>
                                </div>
                            </div>
                            <div class="col mb-3">
                                <label class="form-label fw-bold">Promotion</label>
                                <div class="form-check">
                                    <input v-model="form.featured"
                                           class="form-check-input"
                                           type="checkbox"
                                           id="featured" />
                                    <label class="form-check-label" for="featured">Featured</label>
                                </div>
                            </div>
                        </div>

                        <!-- Actions -->
                        <div class="d-flex justify-content-between">
                            <button type="button" class="btn btn-secondary" @click="cancel">
                                Cancel
                            </button>
                            <button type="submit" class="btn btn-success" :disabled="saving">
                                <span v-if="saving" class="spinner-border spinner-border-sm me-2"></span>
                                Save Asset
                            </button>
                        </div>
                    </form>
                </div>
            </div>
        </div>
    </div>
</template>

<script>
    import Navbar from '../components/Navbar.vue';
    import { ref, onMounted } from 'vue';
    import { fetchLookup, createItem } from '../services/api';
    import { useRouter } from 'vue-router';

    export default {
        name: 'AddAsset',
        components: { Navbar },
        setup() {
            const router = useRouter();
            const saving = ref(false);

            // form state
            const form = ref({
                title: '',
                description: '',
                url: '',
                assetTypeId: null,
                featured: false,
                domain: '',
                division: '',
                serviceLine: '',
                dataSource: '',
                privacyPhi: false,
            });

            // lookup data for dropdowns
            const lookup = ref({
                domains: [],
                divisions: [],
                serviceLines: [],
                dataSources: [],
                assetTypes: [],
            });

            onMounted(async () => {
                lookup.value.domains = await fetchLookup('Domain');
                lookup.value.divisions = await fetchLookup('Division');
                lookup.value.serviceLines = await fetchLookup('ServiceLine');
                lookup.value.dataSources = await fetchLookup('DataSource');
                lookup.value.assetTypes = (await fetchLookup('AssetType'))
                    .filter(x => x.value !== 'Featured'); // remove Featured from asset type options
            });

            // submit handler
            const onLookupChange = (list, idField, textField) => (evt) => {
                const id = Number(evt.target.value);
                const item = lookup.value[list].find(x => x.id === id);
                if (item) {
                    form.value[idField] = id;
                    form.value[textField] = item.value;
                }
            };

            const submitForm = async () => {
                saving.value = true;
                try {
                    await createItem(form.value);
                    // Navigate back to dashboard
                    router.push({ name: 'Dashboard' });
                } catch (err) {
                    console.error('Failed to save:', err);
                    alert('Error saving asset: ' + err.message);
                } finally {
                    saving.value = false;
                }
            };

            const cancel = () => {
                router.back();
            };

            return { form, lookup, saving, submitForm, cancel };
        },
    };
</script>

<style scoped>
    /* Two-column grid */
    .row-custom {
        display: grid;
        grid-template-columns: 1fr 1fr;
        gap: 1rem;
    }

    .asset-span-2 {
        grid-row: span 2;
    }

    /* Make each control full width */
    .row-custom select,
    .row-custom input,
    .row-custom textarea {
        width: 100%;
    }

    /* Taller multi-select */
    .asset-select {
        min-height: 9rem;
    }

    /* Card shadow */
    .shadow-custom {
        box-shadow: 0 10px 20px rgba(0,0,0,0.15), 0 6px 6px rgba(0,0,0,0.1);
    }
</style>
