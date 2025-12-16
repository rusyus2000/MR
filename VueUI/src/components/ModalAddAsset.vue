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

                        <div class="label">Featured</div>
                        <div>
                            <input v-model="form.featured" class="form-check-input" type="checkbox" id="featured" />
                        </div>

                        <div class="label">Title:</div>
                        <div><input v-model="form.title" type="text" class="form-control" required /></div>

                        <div class="label">Description:</div>
                        <div><textarea v-model="form.description" class="form-control" required /></div>

                        <div class="label">URL:</div>
                        <div><input v-model="form.url" type="url" class="form-control" required /></div>

                        <div class="label">Owner:</div>
                        <div>
                            <div class="owner-suggest">
                                <input v-model="ownerQuery"
                                       @input="onOwnerQuery"
                                       @keydown="onOwnerKeydown"
                                       type="text"
                                       placeholder="Search owner by name or email"
                                       class="form-control mb-2"
                                       :disabled="!!form.ownerId" />
                                <div v-if="ownerSuggestions.length && !form.ownerId" class="suggest-box list-group" ref="ownerSuggestBox">
                                    <button type="button"
                                            v-for="(o, idx) in ownerSuggestions"
                                            :key="o.id"
                                            class="list-group-item list-group-item-action"
                                            :class="{ active: idx === ownerActiveIndex }"
                                            @click="selectOwner(o)">
                                        {{ o.name }} ({{ o.email }})
                                    </button>
                                </div>
                            </div>
                            <div class="d-flex gap-2 align-items-center mb-2">
                                <input v-model="form.ownerName" type="text" placeholder="Owner name" class="form-control" :disabled="!!form.ownerId" />
                                <input v-model="form.ownerEmail" type="email" placeholder="Owner email" class="form-control" :disabled="!!form.ownerId" />
                                <button v-if="form.ownerId" type="button" class="btn btn-outline-secondary" @click="clearOwner">Clear</button>
                            </div>
                        </div>

                        <!-- Governance fields -->
                        <div class="label">Operating Entity:</div>
                        <div>
                            <select v-model.number="form.operatingEntityId" @change="() => onLookupChange('operatingEntities','operatingEntityId','operatingEntity')" class="form-select">
                                <option v-for="opt in lookup.operatingEntities" :key="opt.id" :value="opt.id">{{ opt.value }}</option>
                            </select>
                        </div>

                        <div class="label">Executive Sponsor:</div>
                        <div>
                            <div class="d-flex gap-2 align-items-center mb-2">
                                <input v-model="form.executiveSponsorName" type="text" placeholder="Sponsor name" class="form-control" />
                                <input v-model="form.executiveSponsorEmail" type="email" placeholder="Sponsor email" class="form-control" />
                            </div>
                            <small class="text-muted">Optional. Provide both name and email to add a new sponsor.</small>
                        </div>

                        <div class="label">Data Consumers:</div>
                        <div>
                            <textarea v-model="form.dataConsumers" class="form-control" placeholder="Enter data consumers (free text)"></textarea>
                        </div>

                        <div class="label">Refresh Frequency:</div>
                        <div>
                            <select v-model.number="form.refreshFrequencyId" @change="() => onLookupChange('refreshFrequencies','refreshFrequencyId','refreshFrequency')" class="form-select">
                                <option v-for="opt in lookup.refreshFrequencies" :key="opt.id" :value="opt.id">{{ opt.value }}</option>
                            </select>
                        </div>

                        <div class="label">Flags:</div>
                        <div class="flags">
                            <label class="form-check me-3"><input v-model="form.privacyPhi" class="form-check-input" type="checkbox" /> <span class="form-check-label">PHI</span></label>
                            <label class="form-check me-3"><input v-model="form.privacyPii" class="form-check-input" type="checkbox" /> <span class="form-check-label">PII</span></label>
                            <label class="form-check"><input v-model="form.hasRls" class="form-check-input" type="checkbox" /> <span class="form-check-label">RLS</span></label>
                        </div>

                        <div class="label">Last Modified Date:</div>
                        <div><input type="date" class="form-control" v-model="form.lastModifiedDate" /></div>

                        <div class="label">Dependencies:</div>
                        <div>
                            <textarea v-model="form.dependencies" class="form-control" placeholder="Describe upstream data lineage or dependencies"></textarea>
                        </div>

                        <div class="label">Default AD Groups:</div>
                        <div>
                            <textarea v-model="form.defaultAdGroupNames" class="form-control" placeholder="One per line or comma-separated"></textarea>
                        </div>

                        <div class="label">Status:</div>
                        <div>
                            <select v-model.number="form.statusId" class="form-select">
                                <option v-for="opt in lookup.statuses" :key="opt.id" :value="opt.id">{{ opt.value }}</option>
                            </select>
                        </div>

                        <div class="label">Asset Type:</div>
                        <div>
                            <select v-model.number="form.assetTypeId" class="form-select" required>
                                <option v-for="opt in lookup.assetTypes" :key="opt.id" :value="opt.id">
                                    {{ opt.value }}
                                </option>
                            </select>
                        </div>

                        <div class="label">Domain:</div>
                        <div>
                            <select v-model.number="form.domainId" @change="() => onLookupChange('domains','domainId','domain')" class="form-select" required>
                                <option v-for="opt in lookup.domains" :key="opt.id" :value="opt.id">
                                    {{ opt.value }}
                                </option>
                            </select>
                        </div>

                        <div class="label">Division:</div>
                        <div>
                            <select v-model.number="form.divisionId" @change="() => onLookupChange('divisions','divisionId','division')" class="form-select" required>
                                <option v-for="opt in lookup.divisions" :key="opt.id" :value="opt.id">
                                    {{ opt.value }}
                                </option>
                            </select>
                        </div>

                        <div class="label">Service Line:</div>
                        <div>
                            <select v-model.number="form.serviceLineId" @change="() => onLookupChange('serviceLines','serviceLineId','serviceLine')" class="form-select" required>
                                <option v-for="opt in lookup.serviceLines" :key="opt.id" :value="opt.id">
                                    {{ opt.value }}
                                </option>
                            </select>
                        </div>

                        <div class="label">Data Source:</div>
                        <div>
                            <select v-model.number="form.dataSourceId" @change="() => onLookupChange('dataSources','dataSourceId','dataSource')" class="form-select" required>
                                <option v-for="opt in lookup.dataSources" :key="opt.id" :value="opt.id">
                                    {{ opt.value }}
                                </option>
                            </select>
                        </div>

                        <div class="label">Tags:</div>
                        <div>
                            <div class="d-flex mb-2">
                                <input v-model="newTag" @keydown.enter.prevent="addTag" placeholder="Add a tag and press Enter" class="form-control me-2" />
                                <button type="button" class="btn btn-outline-primary" @click="addTag">Add</button>
                            </div>
                            <div class="tag-list">
                                <span v-for="(t, idx) in form.tags" :key="idx" class="tag-chip me-1">
                                    <span class="tag-text">{{ t }}</span>
                                    <button type="button" class="tag-remove" @click="removeTag(idx)" aria-label="Remove tag">×</button>
                                </span>
                            </div>
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
    import { ref, onMounted, watch, computed, nextTick } from 'vue'
    import { fetchLookup, fetchLookupsBulk, createItem, updateItem, searchOwners, fetchCurrentUser } from '../services/api'

    const emit = defineEmits(['close', 'saved'])
    const props = defineProps({
        editItem: { type: Object, default: null }
    })

    const saving = ref(false)

    const form = ref({
        title: '',
        description: '',
        url: '',
        assetTypeId: null,
        tags: [],
        featured: false,
        domain: '',
        division: '',
        serviceLine: '',
        dataSource: '',
        domainId: null,
        divisionId: null,
        serviceLineId: null,
        dataSourceId: null,
        privacyPhi: false,
        privacyPii: false,
        hasRls: false,
        ownerId: null,
        ownerName: '',
        ownerEmail: '',
        statusId: null,
        operatingEntityId: null,
        operatingEntity: '',
        executiveSponsorId: null,
        executiveSponsorName: '',
        executiveSponsorEmail: '',
        dataConsumers: '',
        refreshFrequencyId: null,
        refreshFrequency: '',
        lastModifiedDate: null,
        dependencies: '',
        defaultAdGroupNames: '',
    })

    const lookup = ref({
        domains: [],
        divisions: [],
        serviceLines: [],
        dataSources: [],
        assetTypes: [],
        statuses: [],
        operatingEntities: [],
        refreshFrequencies: [],
    })

    // Admin-only access to this modal is enforced by parent; no admin checks here
    const ownerQuery = ref('')
    const ownerSuggestions = ref([])
    const ownerActiveIndex = ref(-1)
    const ownerSuggestBox = ref(null)

    onMounted(async () => {
        // Bulk-load all lookups in one call
        const bulk = await (await import('../services/api')).getLookupsBulkCached()
        const norm = (arr) => (arr || []).map(x => ({ id: x.id ?? x.Id, value: x.value ?? x.Value }))
        lookup.value.domains = norm(bulk.Domain)
        lookup.value.divisions = norm(bulk.Division)
        lookup.value.serviceLines = norm(bulk.ServiceLine)
        lookup.value.dataSources = norm(bulk.DataSource)
        lookup.value.assetTypes = norm(bulk.AssetType).filter(x => (x.value) !== 'Featured')
        lookup.value.statuses = norm(bulk.Status)
        lookup.value.operatingEntities = norm(bulk.OperatingEntity)
        lookup.value.refreshFrequencies = norm(bulk.RefreshFrequency)
        // Data Consumers are free text; no lookup to load
        // Prefill when editing
        if (props.editItem) {
            const it = props.editItem
            form.value.title = it.title
            form.value.description = it.description
            form.value.url = it.url
            form.value.assetTypeId = it.assetTypeId || null
            form.value.featured = !!it.featured
            form.value.domain = it.domain || ''
            form.value.division = it.division || ''
            form.value.serviceLine = it.serviceLine || ''
            form.value.dataSource = it.dataSource || ''
            form.value.domainId = it.domainId || null
            form.value.divisionId = it.divisionId || null
            form.value.serviceLineId = it.serviceLineId || null
            form.value.dataSourceId = it.dataSourceId || null
            form.value.privacyPhi = !!it.privacyPhi
            form.value.statusId = it.statusId || null
            form.value.ownerId = it.ownerId || null
            form.value.ownerName = it.ownerName || ''
            form.value.ownerEmail = it.ownerEmail || ''
            form.value.tags = Array.isArray(it.tags) ? [...it.tags] : []
            if (it.ownerName || it.ownerEmail) {
                ownerQuery.value = `${it.ownerName || ''}${it.ownerEmail ? ` (${it.ownerEmail})` : ''}`.trim()
            }
            form.value.operatingEntityId = it.operatingEntityId || null
            form.value.operatingEntity = it.operatingEntity || ''
            form.value.refreshFrequencyId = it.refreshFrequencyId || null
            form.value.refreshFrequency = it.refreshFrequency || ''
            form.value.lastModifiedDate = it.lastModifiedDate ? it.lastModifiedDate.substring(0,10) : null
            form.value.privacyPii = !!it.privacyPii
            form.value.hasRls = !!it.hasRls
            form.value.dependencies = it.dependencies || ''
            form.value.defaultAdGroupNames = it.defaultAdGroupNames || ''
            form.value.dataConsumers = it.dataConsumers || ''
        }
    })

    function onLookupChange(list, idField, textField, id) {
        const sel = id || form.value[idField];
        const item = lookup.value[list].find(x => x.id === sel);
        if (item) {
            form.value[idField] = sel;
            form.value[textField] = item.value;
        }
    }

    async function submitForm() {
        saving.value = true
        try {
            // If no existing owner selected, require both name and email when any owner info is provided
            if (!form.value.ownerId) {
                const hasAny = (form.value.ownerName || '').trim() || (form.value.ownerEmail || '').trim()
                const hasBoth = (form.value.ownerName || '').trim() && (form.value.ownerEmail || '').trim()
                if (hasAny && !hasBoth) {
                    alert('Please provide both Owner Name and Owner Email, or select an existing owner.')
                    return
                }
            }
            if (props.editItem && props.editItem.id) {
                await updateItem(props.editItem.id, form.value)
            } else {
                await createItem(form.value)
            }
            emit('saved') // will call ItemGrid.handleAssetSaved
        } catch (err) {
            alert('Error saving asset: ' + err.message)
        } finally {
            saving.value = false
        }
    }

    const newTag = ref('')
    // Data Consumers chips removed; using free-text field only

    function addTag() {
        const v = (newTag.value || '').trim()
        if (!v) return
        if (!form.value.tags.includes(v)) {
            form.value.tags.push(v)
        }
        newTag.value = ''
    }

    function removeTag(i) {
        form.value.tags.splice(i, 1)
    }

    // No add/remove consumer helpers

    async function onOwnerQuery() {
        const q = (ownerQuery.value || '').trim()
        if (!q) { ownerSuggestions.value = []; return }
        try {
            ownerSuggestions.value = await searchOwners(q, 10)
            ownerActiveIndex.value = ownerSuggestions.value.length ? 0 : -1
        } catch (e) { ownerSuggestions.value = [] }
    }

    function selectOwner(o) {
        form.value.ownerId = o.id
        form.value.ownerName = o.name
        form.value.ownerEmail = o.email
        ownerSuggestions.value = []
        ownerQuery.value = `${o.name} (${o.email})`
        ownerActiveIndex.value = -1
    }

    function clearOwner() {
        form.value.ownerId = null
        ownerQuery.value = ''
    }

    function onOwnerKeydown(e) {
        if (form.value.ownerId) return;
        if (!ownerSuggestions.value.length) return;
        if (e.key === 'ArrowDown') {
            e.preventDefault()
            ownerActiveIndex.value = (ownerActiveIndex.value + 1) % ownerSuggestions.value.length
            ensureOwnerActiveVisible()
        } else if (e.key === 'ArrowUp') {
            e.preventDefault()
            ownerActiveIndex.value = (ownerActiveIndex.value <= 0)
                ? ownerSuggestions.value.length - 1
                : ownerActiveIndex.value - 1
            ensureOwnerActiveVisible()
        } else if (e.key === 'Enter') {
            if (ownerActiveIndex.value >= 0) {
                e.preventDefault()
                const o = ownerSuggestions.value[ownerActiveIndex.value]
                if (o) selectOwner(o)
            }
        }
    }

    function ensureOwnerActiveVisible() {
        nextTick(() => {
            const box = ownerSuggestBox.value
            if (!box) return
            const idx = ownerActiveIndex.value
            if (idx < 0) return
            const btn = box.querySelectorAll('.list-group-item')[idx]
            if (btn && btn.scrollIntoView) btn.scrollIntoView({ block: 'nearest' })
        })
    }

    // If user starts typing manual owner fields, treat as new owner (clear selected id)
    watch(() => form.value.ownerName, (v) => {
        if (form.value.ownerId && (v || '').trim()) {
            form.value.ownerId = null
            ownerQuery.value = ''
        }
    })
    watch(() => form.value.ownerEmail, (v) => {
        if (form.value.ownerId && (v || '').trim()) {
            form.value.ownerId = null
            ownerQuery.value = ''
        }
    })
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
        width: 900px;
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

    .modal-body { padding: 1.5rem; max-height: 75vh; overflow: auto; }

    .owner-suggest { position: relative; }
    .suggest-box { position: absolute; top: 100%; left: 0; right: 0; background: #fff; border: 1px solid #dee2e6; z-index: 10; max-height: 200px; overflow: auto; }

    .details-grid { display: grid; grid-template-columns: max-content 1fr max-content 1fr; row-gap: 0.75rem; column-gap: 1rem; align-items: center; }

    .label {
        font-weight: 600;
        white-space: nowrap;
    }
    .flags { display: flex; gap: 1rem; align-items: center; }
    .flags { display: flex; gap: 1rem; align-items: center; }
    .tag-list {
        display: flex;
        flex-wrap: wrap;
        gap: 0.5rem;
    }

    .tag-chip {
        display: inline-flex;
        align-items: center;
        gap: 0.5rem;
        padding: 0.25rem 0.5rem;
        border-radius: 0.35rem;
        background-color: #eaf6ff; /* very light bluish */
        border: 1px solid #c7e6ff; /* slightly darker border */
        color: #05567a;
        font-size: 0.85rem;
    }

    .tag-chip .tag-text {
        line-height: 1;
        display: inline-block;
        vertical-align: middle;
    }

    .tag-remove {
        background: transparent;
        border: none;
        color: #05567a;
        font-weight: 600;
        cursor: pointer;
        padding: 0 0.25rem;
        line-height: 1;
    }

    .tag-remove:focus {
        outline: none;
    }
</style>
