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
        
    const simulation = d3.forceSimulation(nodes)
        .force('link', d3.forceLink(links).id(d => d.id).distance(35))
        .force('charge', d3.forceManyBody().strength(-80))
        .force('center', d3.forceCenter(width / 2, height / 2))
        .force('collision', d3.forceCollide().radius(12));
        
    const link = svg.selectAll('.mini-link')
        .data(links)
        .enter().append('line')
        .attr('class', 'mini-link');
        
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

function openFullGraph() {
    // Future: could open a modal with full interactive graph
    // For now, show a subtle notification
    const icon = document.querySelector('.graph-icon');
    const originalText = icon.textContent;
    
    icon.textContent = '✓';
    icon.style.background = '#28a745';
    icon.style.color = 'white';
    
    setTimeout(() => {
        icon.textContent = originalText;
        icon.style.background = '';
        icon.style.color = '';
    }, 1000);
    
    console.log('Full graph exploration - coming soon!');
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', initializeKnowledgeGraph);