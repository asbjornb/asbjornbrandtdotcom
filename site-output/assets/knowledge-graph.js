// Knowledge Graph JavaScript functionality
function initializeKnowledgeGraph() {
    if (!document.getElementById('mini-graph')) {
        return;
    }

    // Load graph data and render mini visualization
    fetch('/assets/graph-data.json')
        .then(response => response.json())
        .then(data => {
            renderMiniGraph(data);
        })
        .catch(error => {
            console.error('Failed to load graph data:', error);
            // Hide the entire connections section if data loading fails
            const connectionsSection = document.querySelector('.connections-section');
            if (connectionsSection) {
                connectionsSection.style.display = 'none';
            }
        });
}

function renderMiniGraph(graphData) {
    const currentNoteId = document.getElementById('mini-graph').dataset.currentNote;
    const container = d3.select('#mini-graph');
    const containerRect = container.node().getBoundingClientRect();
    
    const width = containerRect.width;
    const height = 150;
    
    // Filter to show current note and its immediate connections
    const currentNode = graphData.nodes.find(n => n.id === currentNoteId);
    if (!currentNode) {
        // Hide the graph container if current note not found
        document.querySelector('.mini-graph-container').style.display = 'none';
        return;
    }
    
    const connectedLinks = graphData.links.filter(l => 
        l.source === currentNoteId || l.target === currentNoteId
    );
    
    // If no connections, hide the graph but keep the connected notes section
    if (connectedLinks.length === 0) {
        document.querySelector('.mini-graph-container').style.display = 'none';
        return;
    }
    
    const connectedNodeIds = new Set([currentNoteId]);
    connectedLinks.forEach(l => {
        connectedNodeIds.add(l.source);
        connectedNodeIds.add(l.target);
    });
    
    const nodes = graphData.nodes.filter(n => connectedNodeIds.has(n.id));
    const links = connectedLinks;
    
    // Clear any existing content
    container.selectAll('*').remove();
    
    const svg = container.append('svg')
        .attr('width', width)
        .attr('height', height);
    
    // Define a single arrow marker that will work for all link types
    svg.append('defs')
        .append('marker')
        .attr('id', 'arrowhead')
        .attr('viewBox', '0 -5 10 10')
        .attr('refX', 12)
        .attr('refY', 0)
        .attr('markerWidth', 6)
        .attr('markerHeight', 6)
        .attr('orient', 'auto')
        .append('path')
        .attr('d', 'M0,-5L10,0L0,5')
        .style('fill', '#666')
        .style('stroke', 'none');
        
    const simulation = d3.forceSimulation(nodes)
        .force('link', d3.forceLink(links).id(d => d.id).distance(35))
        .force('charge', d3.forceManyBody().strength(-80))
        .force('center', d3.forceCenter(width / 2, height / 2))
        .force('collision', d3.forceCollide().radius(12));
        
    const link = svg.selectAll('.mini-link')
        .data(links)
        .enter().append('line')
        .attr('class', 'mini-link')
        .style('stroke', d => {
            switch(d.type) {
                case 'hierarchical': return '#e74c3c';
                case 'reference': return '#4a90e2';
                case 'related': return '#27ae60';
                case 'external': return '#999';
                default: return '#4a90e2'; // Default to blue for reference links
            }
        })
        .style('stroke-width', d => d.type === 'hierarchical' ? 2 : 1.5)
        .attr('marker-end', 'url(#arrowhead)');
        
    // Add tooltips to links
    link.append('title')
        .text(d => {
            const linkTypes = {
                'reference': 'Reference link (blue)',
                'hierarchical': 'Hierarchical connection (red)',
                'related': 'Related content (green)',
                'external': 'External link (gray)'
            };
            return linkTypes[d.type] || 'Connection';
        });
        
    const node = svg.selectAll('.mini-node')
        .data(nodes)
        .enter().append('circle')
        .attr('class', 'mini-node')
        .attr('r', d => d.id === currentNoteId ? 8 : 6)
        .style('fill', d => d.id === currentNoteId ? '#ff6b6b' : '#4ecdc4');
        
    // Only show labels for the current node and if there's enough space
    const label = svg.selectAll('.mini-label')
        .data(nodes.filter(n => n.id === currentNoteId || nodes.length <= 4))
        .enter().append('text')
        .attr('class', 'mini-label')
        .text(d => d.title.length > 12 ? d.title.substring(0, 12) + '…' : d.title)
        .attr('dy', d => d.id === currentNoteId ? -12 : 3);
        
    simulation.on('tick', () => {
        link
            .attr('x1', d => d.source.x)
            .attr('y1', d => d.source.y)
            .attr('x2', d => d.target.x)
            .attr('y2', d => d.target.y);
            
        node
            .attr('cx', d => d.x)
            .attr('cy', d => d.y);
            
        label
            .attr('x', d => d.x)
            .attr('y', d => d.y);
    });
    
    // Add click handler to nodes
    node.on('click', (event, d) => {
        if (d.id !== currentNoteId) {
            window.location.href = d.url;
        }
    }).style('cursor', d => d.id !== currentNoteId ? 'pointer' : 'default');
    
    // Add subtle hover effects
    node.on('mouseover', (event, d) => {
        if (d.id !== currentNoteId) {
            d3.select(event.target).style('opacity', 0.8);
        }
    }).on('mouseout', (event, d) => {
        d3.select(event.target).style('opacity', 1);
    });
}

// Global graph modal functions
function openGlobalGraph() {
    const modal = document.getElementById('global-graph-modal');
    modal.classList.add('active');
    
    // Wait for modal to render before calculating dimensions and loading graph
    setTimeout(() => {
        fetch('/assets/graph-data.json')
            .then(response => response.json())
            .then(data => {
                console.log('Graph data loaded:', data.nodes.length, 'nodes,', data.links.length, 'links');
                renderGlobalGraph(data);
            })
            .catch(error => {
                console.error('Failed to load global graph data:', error);
            });
    }, 100);
}

function closeGlobalGraph() {
    const modal = document.getElementById('global-graph-modal');
    modal.classList.remove('active');
    
    // Clear the global graph to free memory
    const container = d3.select('#global-graph');
    container.selectAll('*').remove();
}

// Global variables for zoom control
let globalSvg = null;
let globalZoom = null;

function renderGlobalGraph(graphData) {
    console.log('Starting renderGlobalGraph');
    const container = d3.select('#global-graph');
    const containerNode = container.node();
    
    if (!containerNode) {
        console.error('Global graph container not found');
        return;
    }
    
    const containerRect = containerNode.getBoundingClientRect();
    let width = containerRect.width;
    let height = containerRect.height;
    
    console.log('Container dimensions from getBoundingClientRect:', width, 'x', height);
    
    // Fallback to parent dimensions or viewport if container dimensions are zero
    if (width === 0 || height === 0) {
        const modal = document.getElementById('global-graph-modal');
        if (modal) {
            const modalRect = modal.getBoundingClientRect();
            width = modalRect.width - 280; // Account for controls panel and padding
            height = modalRect.height - 120; // Account for header and padding
            console.log('Using fallback dimensions:', width, 'x', height);
        }
    }
    
    // Final fallback to reasonable defaults
    if (width <= 0) width = 800;
    if (height <= 0) height = 600;
    
    console.log('Final dimensions:', width, 'x', height);
    
    // Clear any existing content
    container.selectAll('*').remove();
    
    const svg = container.append('svg')
        .attr('width', width)
        .attr('height', height)
        .style('border', '2px solid red') // Temporary debug border
        .style('background', '#f0f0f0'); // Temporary debug background
    
    console.log('SVG created with dimensions:', width, 'x', height);
    
    // Store reference for zoom controls
    globalSvg = svg;
    
    // Add zoom behavior
    const zoom = d3.zoom()
        .scaleExtent([0.1, 4])
        .on('zoom', (event) => {
            g.attr('transform', event.transform);
        });
    
    globalZoom = zoom;
    svg.call(zoom);
    
    // Create a group for all graph elements
    const g = svg.append('g');
    
    // Define arrow marker
    svg.append('defs')
        .append('marker')
        .attr('id', 'global-arrowhead')
        .attr('viewBox', '0 -5 10 10')
        .attr('refX', 15)
        .attr('refY', 0)
        .attr('markerWidth', 8)
        .attr('markerHeight', 8)
        .attr('orient', 'auto')
        .append('path')
        .attr('d', 'M0,-5L10,0L0,5')
        .style('fill', '#666')
        .style('stroke', 'none');
    
    // Use all nodes and links for the global view
    const nodes = [...graphData.nodes];
    const allLinks = [...graphData.links];
    
    // Create a set of valid node IDs for quick lookup
    const validNodeIds = new Set(nodes.map(n => n.id));
    
    // Filter out links that reference non-existent nodes
    const links = allLinks.filter(link => {
        const sourceValid = validNodeIds.has(link.source);
        const targetValid = validNodeIds.has(link.target);
        if (!sourceValid || !targetValid) {
            console.warn('Filtering out invalid link:', link.source, '->', link.target, 
                        'Source exists:', sourceValid, 'Target exists:', targetValid);
        }
        return sourceValid && targetValid;
    });
    
    console.log('Creating simulation with', nodes.length, 'nodes and', links.length, 'valid links (filtered from', allLinks.length, 'total)');
    
    // Give nodes initial random positions to ensure they're visible
    nodes.forEach((d, i) => {
        d.x = width/2 + (Math.random() - 0.5) * 200;
        d.y = height/2 + (Math.random() - 0.5) * 200;
    });
    
    // Create simulation
    const simulation = d3.forceSimulation(nodes)
        .force('link', d3.forceLink(links).id(d => d.id).distance(80))
        .force('charge', d3.forceManyBody().strength(-300))
        .force('center', d3.forceCenter(width / 2, height / 2))
        .force('collision', d3.forceCollide().radius(d => getNodeRadius(d) + 5));
    
    // Create links
    const link = g.selectAll('.global-link')
        .data(links)
        .enter().append('line')
        .attr('class', 'global-link')
        .attr('x1', width/2)
        .attr('y1', height/2)
        .attr('x2', width/2)
        .attr('y2', height/2)
        .style('stroke', d => getLinkColor(d.type))
        .style('stroke-width', d => d.type === 'hierarchical' ? 3 : 2)
        .style('opacity', 0.7)
        .attr('marker-end', 'url(#global-arrowhead)');
    
    console.log('Created', link.size(), 'link elements');
    
    // Create nodes
    const node = g.selectAll('.global-node')
        .data(nodes)
        .enter().append('circle')
        .attr('class', 'global-node')
        .attr('r', d => getNodeRadius(d))
        .attr('cx', d => d.x || width/2)
        .attr('cy', d => d.y || height/2)
        .style('fill', d => getNodeColor(d))
        .style('stroke', '#fff')
        .style('stroke-width', 2)
        .style('cursor', 'pointer')
        .call(d3.drag()
            .on('start', dragStarted)
            .on('drag', dragged)
            .on('end', dragEnded));
    
    console.log('Created', node.size(), 'node elements');
    
    // Add labels
    const label = g.selectAll('.global-label')
        .data(nodes)
        .enter().append('text')
        .attr('class', 'global-label')
        .attr('x', d => d.x || width/2)
        .attr('y', d => (d.y || height/2) + 4)
        .text(d => d.title)
        .style('font-size', '12px')
        .style('font-family', 'sans-serif')
        .style('fill', '#333')
        .style('text-anchor', 'middle')
        .style('pointer-events', 'none')
        .style('user-select', 'none');
    
    // Add tooltips
    node.append('title')
        .text(d => `${d.title}\nType: ${d.type}\nCategory: ${d.category}\nConnections: ${links.filter(l => l.source.id === d.id || l.target.id === d.id).length}`);
    
    // Add click handler to navigate to notes
    node.on('click', (event, d) => {
        window.location.href = d.url;
    });
    
    // Update positions on simulation tick
    simulation.on('tick', () => {
        link
            .attr('x1', d => d.source.x)
            .attr('y1', d => d.source.y)
            .attr('x2', d => d.target.x)
            .attr('y2', d => d.target.y);
            
        node
            .attr('cx', d => d.x)
            .attr('cy', d => d.y);
            
        label
            .attr('x', d => d.x)
            .attr('y', d => d.y + 4);
    });
    
    // Drag functions
    function dragStarted(event, d) {
        if (!event.active) simulation.alphaTarget(0.3).restart();
        d.fx = d.x;
        d.fy = d.y;
    }
    
    function dragged(event, d) {
        d.fx = event.x;
        d.fy = event.y;
    }
    
    function dragEnded(event, d) {
        if (!event.active) simulation.alphaTarget(0);
        d.fx = null;
        d.fy = null;
    }
    
    // Update stats display
    updateGraphStats(graphData);
}

// Zoom control functions
function zoomIn() {
    if (globalSvg && globalZoom) {
        globalSvg.transition().duration(300).call(
            globalZoom.scaleBy, 1.5
        );
    }
}

function zoomOut() {
    if (globalSvg && globalZoom) {
        globalSvg.transition().duration(300).call(
            globalZoom.scaleBy, 0.67
        );
    }
}

function resetZoom() {
    if (globalSvg && globalZoom) {
        globalSvg.transition().duration(500).call(
            globalZoom.transform,
            d3.zoomIdentity
        );
    }
}

function updateGraphStats(graphData) {
    const statsContainer = document.getElementById('graph-stats');
    if (statsContainer) {
        const stats = graphData.stats;
        statsContainer.innerHTML = `
            <strong>Graph Statistics</strong><br>
            Nodes: ${stats.totalNodes}<br>
            Links: ${stats.totalLinks}<br>
            Hubs: ${stats.hubNodes}<br>
            Categories: ${stats.categoryNodes}
        `;
    }
}

// Helper functions for global graph
function getNodeRadius(node) {
    switch(node.type) {
        case 'hub': return 12;
        case 'category': return 8;
        default: return 6;
    }
}

function getNodeColor(node) {
    switch(node.type) {
        case 'hub': return '#e74c3c';
        case 'category': return '#f39c12';
        default: return '#3498db';
    }
}

function getLinkColor(type) {
    switch(type) {
        case 'hierarchical': return '#e74c3c';
        case 'reference': return '#4a90e2';
        case 'related': return '#27ae60';
        case 'external': return '#999';
        default: return '#4a90e2';
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', initializeKnowledgeGraph);

// Add keyboard support for closing modal
document.addEventListener('keydown', (event) => {
    if (event.key === 'Escape') {
        const modal = document.getElementById('global-graph-modal');
        if (modal && modal.classList.contains('active')) {
            closeGlobalGraph();
        }
    }
});

// Close modal when clicking outside the graph container
document.addEventListener('click', (event) => {
    const modal = document.getElementById('global-graph-modal');
    if (modal && modal.classList.contains('active') && event.target === modal) {
        closeGlobalGraph();
    }
});