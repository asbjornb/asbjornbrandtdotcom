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
            // Hide the mini-graph container if data loading fails
            const miniGraphContainer = document.querySelector('.mini-graph-container');
            if (miniGraphContainer) {
                miniGraphContainer.style.display = 'none';
            }
        });
}

function renderMiniGraph(graphData) {
    const currentNoteId = document.getElementById('mini-graph').dataset.currentNote;
    const container = d3.select('#mini-graph');
    const containerRect = container.node().getBoundingClientRect();
    
    const width = containerRect.width;
    const height = 200;
    
    // Filter to show current note and its immediate connections
    const currentNode = graphData.nodes.find(n => n.id === currentNoteId);
    if (!currentNode) return;
    
    const connectedLinks = graphData.links.filter(l => 
        l.source === currentNoteId || l.target === currentNoteId
    );
    
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
        
    const simulation = d3.forceSimulation(nodes)
        .force('link', d3.forceLink(links).id(d => d.id).distance(50))
        .force('charge', d3.forceManyBody().strength(-100))
        .force('center', d3.forceCenter(width / 2, height / 2));
        
    const link = svg.selectAll('.mini-link')
        .data(links)
        .enter().append('line')
        .attr('class', 'mini-link');
        
    const node = svg.selectAll('.mini-node')
        .data(nodes)
        .enter().append('circle')
        .attr('class', 'mini-node')
        .attr('r', d => d.id === currentNoteId ? 12 : 8)
        .style('fill', d => d.id === currentNoteId ? '#ff6b6b' : '#4ecdc4');
        
    const label = svg.selectAll('.mini-label')
        .data(nodes)
        .enter().append('text')
        .attr('class', 'mini-label')
        .text(d => d.title.length > 15 ? d.title.substring(0, 15) + '...' : d.title)
        .attr('dy', 4);
        
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
}

function openFullGraph() {
    // For now, just scroll to top. Could open a modal or dedicated page later
    window.scrollTo({ top: 0, behavior: 'smooth' });
    alert('Full graph exploration feature coming soon! This could open a modal with the full interactive graph.');
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', initializeKnowledgeGraph);