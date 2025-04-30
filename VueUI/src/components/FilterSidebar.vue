<template>
    <div class="filter-sidebar">
        <div class="d-flex justify-content-between align-items-center mb-4">
            <h5 class="text-dark">Filter By</h5>
            <a href="#" class="text-teal" @click.prevent="clearAll">Clear all</a>
        </div>

        <!-- Asset Type -->
        <div class="filter-category">
            <h6>Asset Type</h6>
            <div v-for="(type, index) in assetTypes"
                 :key="type.name"
                 class="form-check"
                 v-show="assetTypesShowAll || index < 5">
                <input class="form-check-input"
                       type="checkbox"
                       :id="type.name"
                       v-model="type.checked"
                       @change="emitFilters" />
                <label class="form-check-label" :for="type.name">
                    {{ type.name }} ({{ getCount('assetType', type.name) }})
                </label>
            </div>
            <a v-if="assetTypes.length > 5"
               href="#"
               class="text-teal small"
               @click.prevent="assetTypesShowAll = !assetTypesShowAll">
                {{ assetTypesShowAll ? 'Show less' : 'Show more' }}
            </a>
        </div>

        <!-- Privacy -->
        <div class="filter-category">
            <h6>Privacy</h6>
            <div class="form-check">
                <input class="form-check-input"
                       type="checkbox"
                       id="phi"
                       v-model="filters.privacy.phi"
                       @change="emitFilters" />
                <label class="form-check-label" for="phi">
                    PHI ({{ getCount('privacy', 'phi') }})
                </label>
            </div>
        </div>

        <!-- Domain -->
        <div class="filter-category">
            <h6>Domain</h6>
            <div v-for="(domain, index) in domains"
                 :key="domain.name"
                 class="form-check"
                 v-show="domainsShowAll || index < 5">
                <input class="form-check-input"
                       type="checkbox"
                       :id="domain.name"
                       v-model="domain.checked"
                       @change="emitFilters" />
                <label class="form-check-label" :for="domain.name">
                    {{ domain.name }} ({{ getCount('domain', domain.name) }})
                </label>
            </div>
            <a v-if="domains.length > 5"
               href="#"
               class="text-teal small"
               @click.prevent="domainsShowAll = !domainsShowAll">
                {{ domainsShowAll ? 'Show less' : 'Show more' }}
            </a>
        </div>

        <!-- Division -->
        <div class="filter-category">
            <h6>Division</h6>
            <div v-for="(division, index) in divisions"
                 :key="division.name"
                 class="form-check"
                 v-show="divisionsShowAll || index < 5">
                <input class="form-check-input"
                       type="checkbox"
                       :id="division.name"
                       v-model="division.checked"
                       @change="emitFilters" />
                <label class="form-check-label" :for="division.name">
                    {{ division.name }} ({{ getCount('division', division.name) }})
                </label>
            </div>
            <a v-if="divisions.length > 5"
               href="#"
               class="text-teal small"
               @click.prevent="divisionsShowAll = !divisionsShowAll">
                {{ domainsShowAll ? 'Show less' : 'Show more' }}
            </a>
        </div>

        <!-- Service Line -->
        <div class="filter-category">
            <h6>Service Line</h6>
            <div v-for="(line, index) in serviceLines"
                 :key="line.name"
                 class="form-check"
                 v-show="serviceLinesShowAll || index < 5">
                <input class="form-check-input"
                       type="checkbox"
                       :id="line.name"
                       v-model="line.checked"
                       @change="emitFilters" />
                <label class="form-check-label" :for="line.name">
                    {{ line.name }} ({{ getCount('serviceLine', line.name) }})
                </label>
            </div>
            <a v-if="serviceLines.length > 5"
               href="#"
               class="text-teal small"
               @click.prevent="serviceLinesShowAll = !serviceLinesShowAll">
                {{ serviceLinesShowAll ? 'Show less' : 'Show more' }}
            </a>
        </div>

        <!-- Data Source -->
        <div class="filter-category">
            <h6>Data Source</h6>
            <div v-for="(source, index) in dataSources"
                 :key="source.name"
                 class="form-check"
                 v-show="dataSourcesShowAll || index < 5">
                <input class="form-check-input"
                       type="checkbox"
                       :id="source.name"
                       v-model="source.checked"
                       @change="emitFilters" />
                <label class="form-check-label" :for="source.name">
                    {{ source.name }} ({{ getCount('dataSource', source.name) }})
                </label>
            </div>
            <a v-if="dataSources.length > 5"
               href="#"
               class="text-teal small"
               @click.prevent="dataSourcesShowAll = !dataSourcesShowAll">
                {{ dataSourcesShowAll ? 'Show less' : 'Show more' }}
            </a>
        </div>
    </div>
</template>

<script>
    export default {
        name: 'FilterSidebar',
        props: {
            currentDomain: {
                type: String,
                default: '',
            },
            items: {
                type: Array,
                default: () => [],
            },
        },
        data() {
            return {
                assetTypes: [
                    { name: 'Dashboard', checked: false },
                    { name: 'Application', checked: false },
                    { name: 'Data Model', checked: false },
                    { name: 'Report', checked: false },
                    { name: 'Featured', checked: false },
                ],
                filters: {
                    privacy: {
                        phi: false,
                    },
                },
                domains: [
                    { name: 'Access to Care', checked: false },
                    { name: 'Clinical Operations', checked: false },
                    { name: 'Finance', checked: false },
                    { name: 'Patient Experience', checked: false },
                    { name: 'People & Workforce', checked: false },
                    { name: 'Quality', checked: false },
                    { name: 'Revenue Cycle', checked: false },
                    { name: 'Service Lines', checked: false },
                ],
                divisions: [
                    { name: 'Greater Central Valley', checked: false },
                    { name: 'Greater East Bay', checked: false },
                    { name: 'Greater Sacramento', checked: false },
                    { name: 'Greater San Francisco', checked: false },
                ],
                serviceLines: [
                    { name: 'Behavioral Health', checked: false },
                    { name: 'Cardiology', checked: false },
                    { name: 'Hospital', checked: false },
                    { name: 'Oncology', checked: false },
                    { name: 'Primary Care', checked: false },
                    { name: 'Orthopedics', checked: false },
                ],
                dataSources: [
                    { name: 'Power BI', checked: false },
                    { name: 'Epic', checked: false },
                    { name: 'Tableau', checked: false },
                    { name: 'Web-Based', checked: false },
                ],
                assetTypesShowAll: false,
                domainsShowAll: false,
                divisionsShowAll: false,
                serviceLinesShowAll: false,
                dataSourcesShowAll: false,
            };
        },
        created() {
            if (this.currentDomain) {
                const domain = this.domains.find(d => d.name.toLowerCase() === this.currentDomain.toLowerCase());
                if (domain) {
                    domain.checked = true;
                } else {
                    this.domains.push({
                        name: this.currentDomain,
                        checked: true,
                    });
                }
            }
            this.emitFilters();
        },
        computed: {
            getCount() {
                return (filterType, value) => {
                    if (!this.items || !Array.isArray(this.items)) return 0;
                    return this.items.filter(item => {
                        if (!item) return false;
                        switch (filterType) {
                            case 'assetType':
                                return item.assetTypes.includes(value);
                            case 'privacy':
                                return value === 'phi' && item.privacy?.phi;
                            case 'domain':
                                return item.domain === value;
                            case 'division':
                                return item.division === value;
                            case 'serviceLine':
                                return item.serviceLine === value;
                            case 'dataSource':
                                return item.dataSource === value;
                            default:
                                return false;
                        }
                    }).length;
                };
            },
        },
        methods: {
            emitFilters() {
                const filters = {
                    assetTypes: this.assetTypes.filter(type => type.checked).map(type => type.name),
                    privacy: { phi: this.filters.privacy.phi },
                    domains: this.domains.filter(domain => domain.checked).map(domain => domain.name),
                    divisions: this.divisions.filter(division => division.checked).map(division => division.name),
                    serviceLines: this.serviceLines.filter(line => line.checked).map(line => line.name),
                    dataSources: this.dataSources.filter(source => source.checked).map(source => source.name),
                };
                this.$emit('update:filters', filters);
            },
            clearAll() {
                this.assetTypes.forEach(type => (type.checked = false));
                this.filters.privacy.phi = false;
                this.domains.forEach(domain => {
                    domain.checked = this.currentDomain ? domain.name.toLowerCase() === this.currentDomain.toLowerCase() : false;
                });
                this.divisions.forEach(division => (division.checked = false));
                this.serviceLines.forEach(line => (line.checked = false));
                this.dataSources.forEach(source => (source.checked = false));
                this.emitFilters();
            },
        },
    };
</script>

<style scoped>
    .filter-sidebar {
        padding: 10px; /* Reduce padding to minimize space */
        border-right: 1px solid #e0e0e0;
        height: 100%;
        width: 100%; /* Ensure it takes full width of the column */
        max-width: none; /* Remove max-width to fill the col-md-3 space */
    }

    .filter-category {
        border: 1px solid #e0e0e0;
        border-radius: 4px;
        padding: 8px; /* Reduce padding inside filter categories */
        margin-bottom: 8px;
    }

    h5 {
        font-size: 1.25rem;
        font-weight: 500;
    }

    h6 {
        font-size: 1rem;
        font-weight: 500;
        margin-bottom: 5px;
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

    .text-Teal {
        color: #00A89E;
    }
</style>