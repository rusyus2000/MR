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

                            <div class="col-12 mb-3">
                                <label class="form-label fw-bold me-3">Flags</label>
                                <div class="d-flex align-items-center gap-4">
                                    <label class="form-check d-flex align-items-center gap-2 mb-0">
                                        <input v-model="form.privacyPhi" class="form-check-input" type="checkbox" />
                                        <span class="form-check-label">PHI</span>
                                    </label>
                                    <label class="form-check d-flex align-items-center gap-2 mb-0">
                                        <input v-model="form.privacyPii" class="form-check-input" type="checkbox" />
                                        <span class="form-check-label">PII</span>
                                    </label>
                                    <label class="form-check d-flex align-items-center gap-2 mb-0">
                                        <input v-model="form.hasRls" class="form-check-input" type="checkbox" />
                                        <span class="form-check-label">RLS</span>
                                    </label>
                                </div>
                            </div>
                            <div class="col mb-3">
                                <label class="form-label fw-bold">Operating Entity</label>
                                <select v-model.number="form.operatingEntityId" @change="onLookupChange('operatingEntities','operatingEntityId','operatingEntity')" class="form-select w-100">
                                    <option v-for="opt in lookup.operatingEntities" :key="opt.id" :value="opt.id">{{ opt.value }}</option>
                                </select>
                            </div>

                            <div class="col mb-3">
                                <label class="form-label fw-bold">Refresh Frequency</label>
                                <select v-model.number="form.refreshFrequencyId" @change="onLookupChange('refreshFrequencies','refreshFrequencyId','refreshFrequency')" class="form-select w-100">
                                    <option v-for="opt in lookup.refreshFrequencies" :key="opt.id" :value="opt.id">{{ opt.value }}</option>
                                </select>
                            </div>

                            <div class="col mb-3">
                                <label class="form-label fw-bold">Contains PII</label>
                                <div class="form-check">
                                    <input v-model="form.privacyPii" class="form-check-input" type="checkbox" id="privacyPii" />
                                    <label class="form-check-label" for="privacyPii">Contains PII</label>
                                </div>
                            </div>
                            <div class="col mb-3">
                                <label class="form-label fw-bold">Has RLS</label>
                                <div class="form-check">
                                    <input v-model="form.hasRls" class="form-check-input" type="checkbox" id="hasRls" />
                                    <label class="form-check-label" for="hasRls">Row-Level Security</label>
                                </div>
                            </div>

                            <div class="col mb-3">
                                <label class="form-label fw-bold">Last Modified Date</label>
                                <input v-model="form.lastModifiedDate" type="date" class="form-control" />
                            </div>

                            <div class="col-12 mb-3">
                                <label class="form-label fw-bold">Executive Sponsor</label>
                                <div class="d-flex gap-2">
                                    <input v-model="form.executiveSponsorName" type="text" placeholder="Sponsor name" class="form-control" />
                                    <input v-model="form.executiveSponsorEmail" type="email" placeholder="Sponsor email" class="form-control" />
                                </div>
                            </div>

                            <div class="col-12 mb-3">
                                <label class="form-label fw-bold">Data Consumers</label>
                                <select v-model="form.dataConsumerIds" multiple class="form-select asset-select w-100">
                                    <option v-for="opt in lookup.dataConsumers" :key="opt.id" :value="opt.id">{{ opt.value }}</option>
                                </select>
                            </div>

                            <div class="col-12 mb-3">
                                <label class="form-label fw-bold">Dependencies (data lineage)</label>
                                <textarea v-model="form.dependencies" class="form-control" placeholder="Describe upstream data lineage or dependencies"></textarea>
                            </div>

                            <div class="col-12 mb-3">
                                <label class="form-label fw-bold">Default AD Group Names</label>
                                <textarea v-model="form.defaultAdGroupNames" class="form-control" placeholder="One per line or comma-separated"></textarea>
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
                 domain: '',
                 division: '',
                 serviceLine: '',
                dataSource: '',
                privacyPhi: false,
                privacyPii: false,
                hasRls: false,
                operatingEntityId: null,
                operatingEntity: '',
                refreshFrequencyId: null,
                refreshFrequency: '',
                lastModifiedDate: null,
                executiveSponsorName: '',
                executiveSponsorEmail: '',
                dataConsumerIds: [],
                dependencies: '',
                defaultAdGroupNames: '',
            });

            // lookup data for dropdowns
            const lookup = ref({
                domains: [],
                divisions: [],
                serviceLines: [],
                dataSources: [],
                assetTypes: [],
                operatingEntities: [],
                refreshFrequencies: [],
                dataConsumers: [],
            });

            onMounted(async () => {
                lookup.value.domains = await fetchLookup('Domain');
                lookup.value.divisions = await fetchLookup('Division');
                lookup.value.serviceLines = await fetchLookup('ServiceLine');
                lookup.value.dataSources = await fetchLookup('DataSource');
                lookup.value.assetTypes = (await fetchLookup('AssetType'))
                    .filter(x => x.value !== 'Featured'); // remove Featured from asset type options
                lookup.value.operatingEntities = await fetchLookup('OperatingEntity');
                lookup.value.refreshFrequencies = await fetchLookup('RefreshFrequency');
                lookup.value.dataConsumers = await fetchLookup('DataConsumer');
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
