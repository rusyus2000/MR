
    <template>
        <div class="card border-0 shadow-custom">
            <div class="card-body position-relative">
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
                    <p :class="['card-text', 'text-muted', showAssetType ? 'description-asset' : 'description-domain']">
                        {{ truncatedDescription }}
                    </p>
                    <div v-if="description.length > 170"
                         class="description-full"
                         :class="{ 'description-asset': showAssetType, 'description-domain': !showAssetType }">
                        <p class="card-text text-muted">{{ description }}</p>
                    </div>
                </div>
                <!-- Request Access button for assets -->
                <div v-if="showAssetType && showRequestAccess && !hasAccess" class="request-access-wrapper">
                    <button class="btn btn-sm btn-outline-teal" @click.stop="handleRequestAccess">
                        Request Access
                    </button>
                </div>
            </div>
        </div>
    </template>

    <script>
        export default {
            name: 'DomainCard',
            props: {
                title: {
                    type: String,
                    required: true,
                },
                description: {
                    type: String,
                    required: true,
                },
                assetTypes: {
                    type: Array,
                    default: () => [],
                },
                showAssetType: {
                    type: Boolean,
                    default: true,
                },
                showRequestAccess: {
                    type: Boolean,
                    default: true,
                },
                hasAccess: {
                    type: Boolean,
                    default: false,
                },
            },
            computed: {
                truncatedDescription() {
                    const maxChars = 170;
                    if (this.description.length <= maxChars) {
                        return this.description;
                    }
                    return this.description.substring(0, maxChars) + '...';
                },
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
                        Featured: 'bg-featured',
                    };
                    return colorMap[assetType] || 'bg-teal';
                },
            },
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
        }

        /* Description wrapper to control the height */
        .description-wrapper {
            position: relative;
            flex-grow: 1;
        }

        /* Truncated description */
        .description-asset {
            max-height: 110px;
        }

        .description-domain {
            max-height: 154px;
        }

        /* Full description on hover */
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

            .description-full.description-asset {
                max-height: 110px;
            }

            .description-full.description-domain {
                max-height: 154px;
            }

        /* Show full description on hover */
        .description-wrapper:hover .description-full {
            opacity: 1;
            max-height: 300px;
        }

        /* Asset type tags styling */
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
            background-color: #00A89E;
        }

        .bg-report {
            background-color: #FF6F61;
        }

        .bg-application {
            background-color: #6A5ACD;
        }

        .bg-data-model {
            background-color: #FFA500;
        }

        .bg-featured {
            background-color: #FFD700;
        }

        .bg-teal {
            background-color: #00A89E;
        }

        /* Request Access button styling */
        .request-access-wrapper {
            position: absolute;
            bottom: 10px;
            right: 10px;
            min-height: 24px;
            z-index: 20;
        }

        .btn-outline-teal {
            border-color: #00A89E;
            color: #00A89E;
            font-size: 0.8rem;
            padding: 0.25rem 0.5rem;
        }

            .btn-outline-teal:hover {
                background-color: #00A89E;
                color: white;
            }

        .shadow-custom {
            box-shadow: 0 10px 20px rgba(0, 0, 0, 0.15), 0 6px 6px rgba(0, 0, 0, 0.1);
        }

        /* Globe icon styling */
        .globe-icon {
            font-size: 1.5rem;
            color: #00A89E;
        }

        /* Ensure description remains left-aligned */
        .card-text {
            text-align: left;
        }
    </style>
