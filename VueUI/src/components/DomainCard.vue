<template>
    <div class="card border-0 shadow-custom">
        <div class="card-body position-relative">
            <!-- Navigation icons: external and detail -->
            <div v-if="(url || id)" class="nav-icons position-absolute top-0 end-0 p-2 d-flex gap-2">
                <a v-if="url" :href="url" target="_blank" rel="noopener" title="Go to Resource">
                    <i class="bi bi-box-arrow-up-right link-icon"></i>
                </a>
                <router-link v-if="id"
                             :to="{ name: 'ItemDetails', params: { id } }"
                             title="More Info">
                    <i class="bi bi-info-circle info-icon"></i>
                </router-link>
            </div>

            <!-- Header with globe icon and title for domains -->
            <div v-if="!showAssetType" class="d-flex align-items-center mb-2">
                <i class="bi bi-globe globe-icon me-2"></i>
                <h5 :class="['card-title', { 'text-center': !showAssetType }]">{{ title }}</h5>
            </div>
            <div v-else class="title-wrapper">
                <h5 class="card-title mt-4">{{ title }}</h5>
            </div>

            <!-- Asset type tags for assets -->
            <div v-if="showAssetType && assetTypes.length" class="asset-type-wrapper">
                <span v-for="(type, index) in assetTypes"
                      :key="index"
                      :class="['badge', getBadgeClass(type), { 'me-1': index < assetTypes.length - 1 }]">
                    {{ type }}
                </span>
            </div>

            <div class="description-wrapper">
                <p :class="[
            'card-text',
            'text-muted',
            showAssetType ? 'description-asset' : 'description-domain'
          ]">
                    {{ truncatedDescription }}
                </p>
                <div v-if="description.length > 170"
                     class="description-full"
                     :class="{
            'description-asset': showAssetType,
            'description-domain': !showAssetType
          }">
                    <p class="card-text text-muted">{{ description }}</p>
                </div>
            </div>

            <!-- Request Access button for assets -->
            <div v-if="showAssetType && showRequestAccess && !hasAccess"
                 class="request-access-wrapper">
                <button class="btn btn-sm btn-outline-teal" @click.stop="handleRequestAccess">
                    Request Access
                </button>
            </div>
        </div>
    </div>
</template>

<script>
    import { RouterLink } from 'vue-router';
    export default {
        name: 'DomainCard',
        components: { RouterLink },
        props: {
            id: {
                type: [String, Number],
                default: null
            },
            url: {
                type: String,
                default: ''
            },
            title: {
                type: String,
                required: true
            },
            description: {
                type: String,
                required: true
            },
            assetTypes: {
                type: Array,
                default: () => []
            },
            showAssetType: {
                type: Boolean,
                default: true
            },
            showRequestAccess: {
                type: Boolean,
                default: true
            },
            hasAccess: {
                type: Boolean,
                default: false
            }
        },
        computed: {
            truncatedDescription() {
                const maxChars = 170;
                return this.description.length <= maxChars
                    ? this.description
                    : this.description.substring(0, maxChars) + '...';
            }
        },
        methods: {
            handleRequestAccess() {
                console.log(`Requesting access for ${this.title}`);
            },
            getBadgeClass(assetType) {
                const colorMap = {
                    Dashboard: 'bg-dashboard',
                    Report: 'bg-report',
                    Application: 'bg-application',
                    'Data Model': 'bg-data-model',
                    Featured: 'bg-featured'
                };
                return colorMap[assetType] || 'bg-teal';
            }
        }
    };
</script>

<style scoped>
    .card {
        border-radius: 10px;
        min-height: 250px;
        height: 250px;
        display: flex;
        flex-direction: column;
        position: relative;
    }

    .card-body {
        padding: 1.5rem;
        position: relative;
        flex: 1;
        display: flex;
        flex-direction: column;
        gap: 0.5rem;
    }

    /** Navigation icons styling **/
    .nav-icons a {
        color: #00a89e;
        font-size: 1.2rem;
    }

        .nav-icons a:hover {
            color: #007f75;
        }

    .card-title {
        font-size: 1.25rem;
        font-weight: 500;
        margin-bottom: 0;
    }

    .title-wrapper {
        min-height: 48px;
    }

    .card-text {
        font-size: 0.9rem;
        line-height: 1.4;
        margin-bottom: 0;
        white-space: normal;
        word-wrap: break-word;
        text-align: left;
    }

    .description-wrapper {
        position: relative;
        flex-grow: 1;
    }

    .description-asset {
        max-height: 110px;
    }

    .description-domain {
        max-height: 154px;
    }

    .description-full {
        position: absolute;
        top: 0;
        left: 0;
        right: 0;
        background-color: #f5f5f5;
        padding: 0.5rem;
        border-radius: 5px;
        box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
        opacity: 0;
        transition: opacity 0.2s ease, max-height 0.3s ease;
        overflow: hidden;
        z-index: 10;
    }

    .description-wrapper:hover .description-full {
        opacity: 1;
        max-height: 300px;
    }

    .asset-type-wrapper {
        position: absolute;
        top: 10px;
        right: 10px;
        display: flex;
        flex-wrap: wrap;
        gap: 0.3rem;
        z-index: 20;
    }

    .badge {
        color: white;
        font-size: 0.65rem;
        padding: 0.3em 0.6em;
    }

    .bg-dashboard {
        background-color: #00a89e;
    }

    .bg-report {
        background-color: #ff6f61;
    }

    .bg-application {
        background-color: #6a5acd;
    }

    .bg-data-model {
        background-color: #ffa500;
    }

    .bg-featured {
        background-color: #ffd700;
    }

    .bg-teal {
        background-color: #00a89e;
    }

    .request-access-wrapper {
        position: absolute;
        bottom: 10px;
        right: 10px;
        z-index: 20;
    }

    .btn-outline-teal {
        border-color: #00a89e;
        color: #00a89e;
        font-size: 0.8rem;
        padding: 0.25rem 0.5rem;
    }

        .btn-outline-teal:hover {
            background-color: #00a89e;
            color: white;
        }

    .shadow-custom {
        box-shadow: 0 10px 20px rgba(0, 0, 0, 0.15), 0 6px 6px rgba(0, 0, 0, 0.1);
    }

    .globe-icon {
        font-size: 1.5rem;
        color: #00a89e;
    }
</style>
