// ==UserScript==
// @name         Reddit to OPML Sync
// @namespace    http://your-api.example.com
// @version      1.0
// @description  Syncs Reddit subscribed subreddits to an OPML API for RSS readers
// @author       You
// @match        *://www.reddit.com/*
// @grant        GM_xmlhttpRequest
// @grant        GM_notification
// @connect      localhost
// @run-at       document-idle
// ==/UserScript==

(function() {
    'use strict';

    // CONFIGURATION - Update these values
    const CONFIG = {
        // Your Reddit username
        redditUsername: 'alephhelix',

        // Your API URL (update this to match your deployment)
        apiUrl: 'http://localhost:8080',

        // Minimum time between syncs in milliseconds (30 minutes)
        syncInterval: 30 * 60 * 1000
    };

    // Storage key for last sync time
    const STORAGE_KEY = 'reddit-to-opml-last-sync';

    /**
     * Check if enough time has passed since last sync
     */
    function shouldSync() {
        const lastSync = localStorage.getItem(STORAGE_KEY);
        if (!lastSync) return true;

        const elapsed = Date.now() - parseInt(lastSync);
        return elapsed >= CONFIG.syncInterval;
    }

    /**
     * Record that a sync just occurred
     */
    function recordSync() {
        localStorage.setItem(STORAGE_KEY, Date.now().toString());
    }

    /**
     * Extract subreddit names from the communities page HTML
     */
    function extractSubreddits(htmlText) {
        const parser = new DOMParser();
        const doc = parser.parseFromString(htmlText, 'text/html');

        // Find all community-management-item elements and extract community-name attribute
        const items = doc.querySelectorAll('community-management-item[community-name]');
        const subreddits = Array.from(items).map(item => {
            const name = item.getAttribute('community-name');
            return name;
        }).filter(name => name !== null);

        return subreddits;
    }

    /**
     * Send subreddits to the API
     */
    function sendToApi(subreddits) {
        GM_xmlhttpRequest({
            method: 'POST',
            url: `${CONFIG.apiUrl}/api/subreddits/sync`,
            headers: {
                'Content-Type': 'application/json'
            },
            data: JSON.stringify(subreddits),
            onload: function(response) {
                if (response.status >= 200 && response.status < 300) {
                    console.log(`[RedditToOPML] Synced ${subreddits.length} subreddits`);
                    GM_notification({
                        text: `Synced ${subreddits.length} subreddits`,
                        timeout: 3000
                    });
                } else {
                    console.error(`[RedditToOPML] API error: ${response.status}`);
                }
            },
            onerror: function(error) {
                console.error('[RedditToOPML] Network error:', error);
            }
        });
    }

    /**
     * Fetch the communities page and sync subreddits
     */
    function syncSubreddits() {
        if (!shouldSync()) {
            console.log('[RedditToOPML] Sync skipped - too soon since last sync');
            return;
        }

        const communitiesUrl = `/user/${CONFIG.redditUsername}/communities/`;

        GM_xmlhttpRequest({
            method: 'GET',
            url: communitiesUrl,
            onload: function(response) {
                if (response.status === 200) {
                    const subreddits = extractSubreddits(response.responseText);
                    if (subreddits.length > 0) {
                        sendToApi(subreddits);
                        recordSync();
                    } else {
                        console.log('[RedditToOPML] No subreddits found');
                    }
                } else if (response.status === 404) {
                    console.error('[RedditToOPML] Communities page not found - check username');
                } else {
                    console.error(`[RedditToOPML] Failed to fetch communities page: ${response.status}`);
                }
            },
            onerror: function(error) {
                console.error('[RedditToOPML] Failed to fetch communities page:', error);
            }
        });
    }

    // Run sync when page is idle
    if (document.readyState === 'complete' || document.readyState === 'interactive') {
        setTimeout(syncSubreddits, 1000);
    } else {
        document.addEventListener('DOMContentLoaded', () => {
            setTimeout(syncSubreddits, 1000);
        });
    }

})();
