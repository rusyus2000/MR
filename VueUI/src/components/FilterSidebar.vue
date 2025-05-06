<template>
    <div class="filter-sidebar">
        <div class="d-flex justify-content-between align-items-center mb-4">
            <h5 class="text-dark">Filter By</h5>
            <a href="#" class="text-teal" @click.prevent="clearAll">Clear all</a>
        </div>

        <!-- Asset Type -->
        <div class="filter-category">
            <h6>Asset Type</h6>
            <div v-for="(type, idx) in assetTypes"
                 :key="type.name"
                 class="form-check"
                 v-show="assetTypesShowAll || idx < 5">
                <input class="form-check-input"
                       type="checkbox"
                       :id="'asset-'+type.name"
                       v-model="type.checked"
                       @change="emitFilters" />
                <label class="form-check-label" :for="'asset-'+type.name">
                    {{ type.name }} ({{ getCount('assetType', type.name) }})
                </label>
            </div>
            <a v-if="assetTypes.length > 5" href="#" class="text-teal small" @click.prevent="assetTypesShowAll = !assetTypesShowAll">
                {{ assetTypesShowAll ? 'Show less' : 'Show more' }}
            </a>
        </div>

        <!-- Privacy -->
        <div class="filter-category">
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
        <div class="filter-category">
            <h6>Domain</h6>
            <div v-for="(domain, idx) in domains"
                 :key="domain.name"
                 class="form-check"
                 v-show="domainsShowAll || idx < 5">
                <input class="form-check-input"
                       type="checkbox"
                       :id="'domain-'+domain.name"
                       v-model="domain.checked"
                       @change="emitFilters" />
                <label class="form-check-label" :for="'domain-'+domain.name">
                    {{ domain.name }} ({{ getCount('domain', domain.name) }})
                </label>
            </div>
            <a v-if="domains.length > 5" href="#" class="text-teal small" @click.prevent="domainsShowAll = !domainsShowAll">
                {{ domainsShowAll ? 'Show less' : 'Show more' }}
            </a>
        </div>

        <!-- Division -->
        <div class="filter-category">
            <h6>Division</h6>
            <div v-for="(division, idx) in divisions"
                 :key="division.name"
                 class="form-check"
                 v-show="divisionsShowAll || idx < 5">
                <input class="form-check-input"
                       type="checkbox"
                       :id="'division-'+division.name"
                       v-model="division.checked"
                       @change="emitFilters" />
                <label class="form-check-label" :for="'division-'+division.name">
                    {{ division.name }} ({{ getCount('division', division.name) }})
                </label>
            </div>
            <a v-if="divisions.length > 5" href="#" class="text-teal small" @click.prevent="divisionsShowAll = !divisionsShowAll">
                {{ divisionsShowAll ? 'Show less' : 'Show more' }}
            </a>
        </div>

        <!-- Service Line -->
        <div class="filter-category">
            <h6>Service Line</h6>
            <div v-for="(line, idx) in serviceLines"
                 :key="line.name"
                 class="form-check"
                 v-show="serviceLinesShowAll || idx < 5">
                <input class="form-check-input"
                       type="checkbox"
                       :id="'service-'+line.name"
                       v-model="line.checked"
                       @change="emitFilters" />
                <label class="form-check-label" :for="'service-'+line.name">
                    {{ line.name }} ({{ getCount('serviceLine', line.name) }})
                </label>
            </div>
            <a v-if="serviceLines.length > 5" href="#" class="text-teal small" @click.prevent="serviceLinesShowAll = !serviceLinesShowAll">
                {{ serviceLinesShowAll ? 'Show less' : 'Show more' }}
            </a>
        </div>

        <!-- Data Source -->
        <div class="filter-category">
            <h6>Data Source</h6>
            <div v-for="(source, idx) in dataSources"
                 :key="source.name"
                 class="form-check"
                 v-show="dataSourcesShowAll || idx < 5">
                <input class="form-check-input"
                       type="checkbox"
                       :id="'source-'+source.name"
                       v-model="source.checked"
                       @change="emitFilters" />
                <label class="form-check-label" :for="'source-'+source.name">
                    {{ source.name }} ({{ getCount('dataSource', source.name) }})
                </label>
            </div>
            <a v-if="dataSources.length > 5" href="#" class="text-teal small" @click.prevent="dataSourcesShowAll = !dataSourcesShowAll">
                {{ dataSourcesShowAll ? 'Show less' : 'Show more' }}
            </a>
        </div>
    </div>
</template>

<script>
    export default {
        name: 'FilterSidebar',
        props: {
            itemsAll: {
                type: Array,
                default: () => [],
            },
            currentDomain: {
                type: String,
                default: '',
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
                    privacy: { phi: false },
                },
                domains: [
                    { name: 'Strategy', checked: false },
                    { name: 'Access to Care', checked: false },
                    { name: 'Quality', checked: false },
                    { name: 'People & Workforce', checked: false },
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
            // Pre-check current domain if provided
            if (this.currentDomain) {
                const d = this.domains.find(x => x.name.toLowerCase() === this.currentDomain.toLowerCase());
                if (d) d.checked = true;
                else this.domains.push({ name: this.currentDomain, checked: true });
            }
            this.emitFilters();
        },
        methods: {
            emitFilters() {
                const out = {
                    assetTypes: this.assetTypes.filter(t => t.checked).map(t => t.name),
                    privacy: { phi: this.filters.privacy.phi },
                    domains: this.domains.filter(d => d.checked).map(d => d.name),
                    divisions: this.divisions.filter(d => d.checked).map(d => d.name),
                    serviceLines: this.serviceLines.filter(s => s.checked).map(s => s.name),
                    dataSources: this.dataSources.filter(ds => ds.checked).map(ds => ds.name),
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
                        case 'assetType': return item.assetTypes.includes(val);
                        case 'privacy': return val === 'phi' && item.privacyPhi;
                        case 'domain': return item.domain === val;
                        case 'division': return item.division === val;
                        case 'serviceLine': return item.serviceLine === val;
                        case 'dataSource': return item.dataSource === val;
                    }
                }).length;
            },
        },
    };
</script>

<style scoped>
    .filter-sidebar {
        padding: 15px;
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
