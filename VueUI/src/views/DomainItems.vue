<template>
    <div>
        <Navbar />
        <HeroSection :domain="domainNameFormatted" />

        <div class="content-area d-flex">
            <div class="filter-col">
                <FilterSidebar :current-domain="domainNameFormatted"
                               :items="items"
                               @update:filters="updateFilters" />
            </div>
            <div class="asset-col flex-grow-1">
                <ItemGrid :filters="selectedFilters" :items="filteredItems" />
            </div>
        </div>
    </div>
</template>

<script>
    import { ref, computed, onMounted } from 'vue';
    import Navbar from '../components/Navbar.vue';
    import HeroSection from '../components/HeroSection.vue';
    import FilterSidebar from '../components/FilterSidebar.vue';
    import ItemGrid from '../components/ItemGrid.vue';
    import { fetchItems } from '../services/api';

    export default {
        name: 'DomainItems',
        components: { Navbar, HeroSection, FilterSidebar, ItemGrid },
        props: {
            domainName: { type: String, required: true },
        },
        setup(props) {
            const items = ref([]);
            const selectedFilters = ref({
                assetTypes: [],
                privacy: { phi: false },
                domains: [],
                divisions: [],
                serviceLines: [],
                dataSources: [],
            });

            const domainNameFormatted = computed(() =>
                props.domainName
                    .replace(/-/g, ' ')
                    .replace(/\b\w/g, c => c.toUpperCase())
            );

            onMounted(async () => {
                // initial load: items scoped to this domain
                items.value = await fetchItems({ domain: domainNameFormatted.value });
            });

            function updateFilters(filters) {
                selectedFilters.value = filters;
            }

            const filteredItems = computed(() =>
                items.value.filter(item => {
                    const f = selectedFilters.value;
                    if (
                        f.assetTypes.length &&
                        !f.assetTypes.some(t => item.assetTypes.includes(t))
                    )
                        return false;
                    if (f.privacy.phi && !item.privacyPhi) return false;
                    if (f.domains.length && !f.domains.includes(item.domain)) return false;
                    if (
                        f.divisions.length &&
                        !f.divisions.includes(item.division)
                    )
                        return false;
                    if (
                        f.serviceLines.length &&
                        !f.serviceLines.includes(item.serviceLine)
                    )
                        return false;
                    if (
                        f.dataSources.length &&
                        !f.dataSources.includes(item.dataSource)
                    )
                        return false;
                    return true;
                })
            );

            return {
                items,
                selectedFilters,
                domainNameFormatted,
                updateFilters,
                filteredItems,
            };
        },
    };
</script>

<style scoped>
    .filter-col {
        width: 220px;
    }

    .asset-col {
        padding: 0 20px;
        display: flex;
        justify-content: center;
    }

    .content-area {
        min-height: calc(100vh - 270px);
    }
</style>
