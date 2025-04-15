<template>
    <div class="filter-sidebar">
        <div class="d-flex justify-content-between align-items-center mb-4">
            <h5 class="text-dark">Filter By</h5>
            <a href="#" class="text-teal" @click.prevent="clearAll">Clear all</a>
        </div>

        <!-- Asset Type -->
        <div class="mb-4">
            <h6>Asset Type</h6>
            <div v-for="type in assetTypes" :key="type.name" class="form-check">
                <input class="form-check-input"
                       type="checkbox"
                       :id="type.name"
                       v-model="type.checked"
                       @change="emitFilters" />
                <label class="form-check-label" :for="type.name">
                    {{ type.name }} ({{ type.count }})
                </label>
            </div>
            <a href="#" class="text-teal small">Show more</a>
        </div>

        <!-- Privacy -->
        <div class="mb-4">
            <h6>Privacy</h6>
            <div class="form-check">
                <input class="form-check-input"
                       type="checkbox"
                       id="phi"
                       v-model="filters.privacy.phi"
                       @change="emitFilters" />
                <label class="form-check-label" for="phi">
                    PHI (3)
                </label>
            </div>
        </div>

        <!-- Domain -->
        <div class="mb-4">
            <h6>Domain</h6>
            <div v-for="domain in domains" :key="domain.name" class="form-check">
                <input class="form-check-input"
                       type="checkbox"
                       :id="domain.name"
                       v-model="domain.checked"
                       @change="emitFilters" />
                <label class="form-check-label" :for="domain.name">
                    {{ domain.name }} ({{ domain.count }})
                </label>
            </div>
            <a href="#" class="text-teal small">Show more</a>
        </div>

        <!-- Division -->
        <div class="mb-4">
            <h6>Division</h6>
            <div v-for="division in divisions" :key="division.name" class="form-check">
                <input class="form-check-input"
                       type="checkbox"
                       :id="division.name"
                       v-model="division.checked"
                       @change="emitFilters" />
                <label class="form-check-label" :for="division.name">
                    {{ division.name }} ({{ division.count }})
                </label>
            </div>
            <a href="#" class="text-teal small">Show more</a>
        </div>

        <!-- Service Line -->
        <div class="mb-4">
            <h6>Service Line</h6>
            <div v-for="line in serviceLines" :key="line.name" class="form-check">
                <input class="form-check-input"
                       type="checkbox"
                       :id="line.name"
                       v-model="line.checked"
                       @change="emitFilters" />
                <label class="form-check-label" :for="line.name">
                    {{ line.name }} ({{ line.count }})
                </label>
            </div>
            <a href="#" class="text-teal small">Show more</a>
        </div>

        <!-- Data Source -->
        <div class="mb-4">
            <h6>Data Source</h6>
            <div v-for="source in dataSources" :key="source.name" class="form-check">
                <input class="form-check-input"
                       type="checkbox"
                       :id="source.name"
                       v-model="source.checked"
                       @change="emitFilters" />
                <label class="form-check-label" :for="source.name">
                    {{ source.name }} ({{ source.count }})
                </label>
            </div>
            <a href="#" class="text-teal small">Show more</a>
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
        },
        data() {
            return {
                assetTypes: [
                    { name: 'Dashboard', count: 8, checked: false },
                    { name: 'Application', count: 1, checked: false },
                    { name: 'Data Model', count: 1, checked: false },
                ],
                filters: {
                    privacy: {
                        phi: false,
                    },
                },
                domains: [
                    { name: 'Strategy', count: 1, checked: false },
                    { name: 'Access to Care', count: 20, checked: false },
                    { name: 'Quality', count: 17, checked: false },
                    { name: 'People & Workforce', count: 16, checked: false },
                ],
                divisions: [
                    { name: 'Greater Central Valley', count: 20, checked: false },
                    { name: 'Greater East Bay', count: 20, checked: false },
                    { name: 'Greater Sacramento', count: 20, checked: false },
                    { name: 'Greater San Francisco', count: 20, checked: false },
                ],
                serviceLines: [
                    { name: 'Behavioral Health', count: 2, checked: false },
                    { name: 'Cardiology', count: 2, checked: false },
                    { name: 'Hospital', count: 2, checked: false },
                    { name: 'Oncology', count: 2, checked: false },
                    { name: 'Primary Care', count: 2, checked: false },
                ],
                dataSources: [
                    { name: 'Power BI', count: 14, checked: false },
                    { name: 'Epic', count: 3, checked: false },
                    { name: 'Tableau', count: 2, checked: false },
                    { name: 'Web-Based', count: 1, checked: false },
                ],
            };
        },
        created() {
            // Pre-check the current domain
            if (this.currentDomain) {
                const domain = this.domains.find(d => d.name.toLowerCase() === this.currentDomain.toLowerCase());
                if (domain) {
                    domain.checked = true;
                } else {
                    this.domains.push({
                        name: this.currentDomain,
                        count: 0, // Adjust count as needed
                        checked: true,
                    });
                }
            }
            // Emit initial filters on page load
            this.emitFilters();
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
                // Reset asset types
                this.assetTypes.forEach(type => (type.checked = false));
                // Reset privacy
                this.filters.privacy.phi = false;
                // Reset domains, but keep the current domain checked
                this.domains.forEach(domain => {
                    domain.checked = this.currentDomain ? domain.name.toLowerCase() === this.currentDomain.toLowerCase() : false;
                });
                // Reset divisions
                this.divisions.forEach(division => (division.checked = false));
                // Reset service lines
                this.serviceLines.forEach(line => (line.checked = false));
                // Reset data sources
                this.dataSources.forEach(source => (source.checked = false));
                // Emit the updated filters
                this.emitFilters();
            },
        },
    };
</script>

<style scoped>
    .filter-sidebar {
        padding: 20px;
        border-right: 1px solid #e0e0e0;
        height: 100%;
    }

    h5 {
        font-size: 1.25rem;
        font-weight: 500;
    }

    h6 {
        font-size: 1rem;
        font-weight: 500;
        margin-bottom: 10px;
    }

    .form-check {
        margin-bottom: 5px;
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