<template>
    <div class="filter-sidebar">
        <div class="d-flex justify-content-between align-items-center mb-4">
            <h5 class="text-dark">Filter By</h5>
            <span v-if="filtersActive"
                  class="search-tag badge d-inline-flex align-items-center"
                  style="cursor: pointer;"
                  @click.prevent="clearAll">
                <span class="me-2">Clear</span>
                <button class="btn-close btn-close-sm" aria-label="Close"></button>
            </span>
        </div>

        <!-- Asset Type -->
        <div v-if="filterSections.assetTypes" class="filter-category">
            <h6>Asset Type</h6>
            <div v-for="(type, idx) in assetTypes"
                 :key="type.id"
                 class="form-check"
                 v-show="assetTypesShowAll || idx < 5">
                <input class="form-check-input"
                       type="checkbox"
                       :id="'asset-'+type.id"
                       v-model="type.checked"
                       @change="emitFilters" />
                <label class="form-check-label" :for="'asset-'+type.id">
                    {{ type.name }} ({{ type.count }})
                </label>
            </div>
            <a v-if="assetTypes.length > 5" href="#" class="text-teal small" @click.prevent="assetTypesShowAll = !assetTypesShowAll">
                {{ assetTypesShowAll ? 'Show less' : 'Show more' }}
            </a>
        </div>

        <!-- Privacy -->
        <div v-if="filterSections.privacy" class="filter-category">
            <h6>Privacy</h6>
            <div class="form-check">
                <input class="form-check-input"
                       type="checkbox"
                       id="privacy-phi"
                       v-model="filters.privacy.phi"
                       @change="emitFilters" />
                <label class="form-check-label" for="privacy-phi">
                    PHI ({{ getCount('privacy', 'phi') }})
                </label>
            </div>
        </div>

        <!-- Domain -->
        <div v-if="filterSections.domain" class="filter-category">
            <h6>Domain</h6>
            <div v-for="(domain, idx) in domains"
                 :key="domain.id"
                 class="form-check"
                 v-show="domainsShowAll || idx < 5">
                <input class="form-check-input"
                       type="checkbox"
                       :id="'domain-'+domain.id"
                       v-model="domain.checked"
                       @change="emitFilters" />
                <label class="form-check-label" :for="'domain-'+domain.id">
                    {{ domain.name }} ({{ domain.count }})
                </label>
            </div>
            <a v-if="domains.length > 5" href="#" class="text-teal small" @click.prevent="domainsShowAll = !domainsShowAll">
                {{ domainsShowAll ? 'Show less' : 'Show more' }}
            </a>
        </div>

        <!-- Division -->
        <div v-if="filterSections.division" class="filter-category">
            <h6>Division</h6>
            <div v-for="(division, idx) in divisions"
                 :key="division.id"
                 class="form-check"
                 v-show="divisionsShowAll || idx < 5">
                <input class="form-check-input"
                       type="checkbox"
                       :id="'division-'+division.id"
                       v-model="division.checked"
                       @change="emitFilters" />
                <label class="form-check-label" :for="'division-'+division.id">
                    {{ division.name }} ({{ division.count }})
                </label>
            </div>
            <a v-if="divisions.length > 5" href="#" class="text-teal small" @click.prevent="divisionsShowAll = !divisionsShowAll">
                {{ divisionsShowAll ? 'Show less' : 'Show more' }}
            </a>
        </div>

        <!-- Service Line -->
        <div v-if="filterSections.serviceLine" class="filter-category">
            <h6>Service Line</h6>
            <div v-for="(line, idx) in serviceLines"
                 :key="line.id"
                 class="form-check"
                 v-show="serviceLinesShowAll || idx < 5">
                <input class="form-check-input"
                       type="checkbox"
                       :id="'service-'+line.id"
                       v-model="line.checked"
                       @change="emitFilters" />
                <label class="form-check-label" :for="'service-'+line.id">
                    {{ line.name }} ({{ line.count }})
                </label>
            </div>
            <a v-if="serviceLines.length > 5" href="#" class="text-teal small" @click.prevent="serviceLinesShowAll = !serviceLinesShowAll">
                {{ serviceLinesShowAll ? 'Show less' : 'Show more' }}
            </a>
        </div>

        <!-- Data Source -->
        <div v-if="filterSections.dataSource" class="filter-category">
            <h6>Data Source</h6>
            <div v-for="(source, idx) in dataSources"
                 :key="source.id"
                 class="form-check"
                 v-show="dataSourcesShowAll || idx < 5">
                <input class="form-check-input"
                       type="checkbox"
                       :id="'source-'+source.id"
                       v-model="source.checked"
                       @change="emitFilters" />
                <label class="form-check-label" :for="'source-'+source.id">
                    {{ source.name }} ({{ source.count }})
                </label>
            </div>
            <a v-if="dataSources.length > 5" href="#" class="text-teal small" @click.prevent="dataSourcesShowAll = !dataSourcesShowAll">
                {{ dataSourcesShowAll ? 'Show less' : 'Show more' }}
            </a>
        </div>
    </div>
</template>

<script>
    import { FILTER_SECTIONS } from '../config';

    export default {
        name: 'FilterSidebar',
        props: {
            itemsAll: { type: Array, default: () => [] },
            currentDomain: { type: String, default: '' },
            filtersActive: { type: Boolean, default: false }
        },
        data() {
            return {
                filterSections: FILTER_SECTIONS,
                assetTypes: [],
                filters: { privacy: { phi: false } },
                domains: [],
                divisions: [],
                serviceLines: [],
                dataSources: [],
                assetTypesShowAll: false,
                domainsShowAll: false,
                divisionsShowAll: false,
                serviceLinesShowAll: false,
                dataSourcesShowAll: false,
                _initialDomain: null
            };
        },
        created() {
            this._initialDomain = this.currentDomain;
            this.loadLookups();
        },
        mounted() {
            this.$watch(() => ({
                a: this.assetTypes.length,
                d: this.domains.length,
                v: this.divisions.length,
                s: this.serviceLines.length,
                ds: this.dataSources.length
            }), () => this.emitFilters());
        },
        methods: {
            async loadLookups() {
                try {
                    const api = await import('../services/api');
                    const bulk = await api.fetchLookupsCountsBulk(['AssetType','Domain','Division','ServiceLine','DataSource']);
                    const norm = x => ({ id: x.id ?? x.Id, name: x.value ?? x.Value, count: x.count ?? x.Count });
                    const mapArr = arr => (arr || []).map(norm).filter(x => x.count > 0).map(x => ({ id: x.id, name: x.name, checked: false, count: x.count }));
                    this.assetTypes = mapArr(bulk.AssetType);
                    this.domains = mapArr(bulk.Domain);
                    this.divisions = mapArr(bulk.Division);
                    this.serviceLines = mapArr(bulk.ServiceLine);
                    this.dataSources = mapArr(bulk.DataSource);

                    if (this._initialDomain) {
                        const found = this.domains.find(d => d.name.toLowerCase() === this._initialDomain.toLowerCase());
                        if (found) found.checked = true;
                    }
                } catch (err) {
                    console.error('Failed loading lookups', err);
                }
            },

            emitFilters() {
                // Emit lookup IDs (numbers) instead of their display names.
                // The backend will filter by FK IDs (e.g. DomainId) when provided.
                const out = {
                    assetTypes: this.assetTypes.filter(t => t.checked).map(t => t.id),
                    privacy: { phi: this.filters.privacy.phi },
                    domains: this.domains.filter(d => d.checked).map(d => d.id),
                    divisions: this.divisions.filter(d => d.checked).map(d => d.id),
                    serviceLines: this.serviceLines.filter(s => s.checked).map(s => s.id),
                    dataSources: this.dataSources.filter(ds => ds.checked).map(ds => ds.id),
                };
                this.$emit('update:filters', out);
            },

            clearAll() {
                this.assetTypes.forEach(t => (t.checked = false));
                this.filters.privacy.phi = false;
                this.domains.forEach(d => (d.checked = false));
                this.divisions.forEach(d => (d.checked = false));
                this.serviceLines.forEach(s => (s.checked = false));
                this.dataSources.forEach(ds => (ds.checked = false));
                this.emitFilters();
            },

            getCount(type, val) {
                return this.itemsAll.filter(item => {
                    switch (type) {
                        case 'assetType': return (item.assetTypeName || '').toString() === val;
                        case 'privacy': return val === 'phi' && item.privacyPhi;
                        case 'domain': return item.domain === val;
                        case 'division': return item.division === val;
                        case 'serviceLine': return item.serviceLine === val;
                        case 'dataSource': return item.dataSource === val;
                    }
                }).length;
            }
        }
    };
</script>

<style scoped>
    .filter-sidebar {
        padding: 0px 5px 5px 5px;
        border-right: 1px solid #e0e0e0;
        height: 100%;
        width: fit-content;
        width: 250px;
    }

    .filter-category {
        border: 1px solid #e0e0e0;
        border-radius: 4px;
        padding: 10px;
        margin-bottom: 10px;
    }

    .search-tag {
        background-color: #e7f1ff;
        color: #0056b3;
        font-weight: 500;
        padding: 0.4rem 0.6rem;
        border: 1px solid #b6d4fe;
        font-size: 0.85rem;
        border-radius: 0.375rem;
    }
    .btn-close-sm {
        width: 0.75em;
        height: 0.75em;
        font-size: 0.7rem;
    }
    .form-check {
        margin-bottom: 2px;
    }

    .form-check-input {
        margin-top: 3px;
    }

    .form-check-label {
        font-size: 0.9rem;
    }

    .text-teal {
        color: #00A89E;
    }
</style>
