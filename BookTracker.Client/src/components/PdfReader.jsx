import React, { useState, useEffect } from 'react';
import { Document, Page, pdfjs } from 'react-pdf';
import { 
  ChevronLeft, ChevronRight, X, Maximize2, Minimize2, 
  Loader2, Download, ZoomIn, ZoomOut 
} from 'lucide-react';
import { motion, AnimatePresence } from 'framer-motion';

// Configure PDF.js worker
pdfjs.GlobalWorkerOptions.workerSrc = `//unpkg.com/pdfjs-dist@${pdfjs.version}/build/pdf.worker.min.mjs`;

const PdfReader = ({ fileUrl, title, onClose }) => {
  const [numPages, setNumPages] = useState(null);
  const [pageNumber, setPageNumber] = useState(1);
  const [scale, setScale] = useState(1.0);
  const [loading, setLoading] = useState(true);

  function onDocumentLoadSuccess({ numPages }) {
    setNumPages(numPages);
    setLoading(false);
  }

  const changePage = (offset) => {
    setPageNumber(prev => Math.min(Math.max(1, prev + offset), numPages));
  };

  const handleZoom = (delta) => {
    setScale(prev => Math.min(Math.max(0.5, prev + delta), 2.5));
  };

  // Keyboard navigation
  useEffect(() => {
    const handleKeyDown = (e) => {
      if (e.key === 'ArrowRight') changePage(1);
      if (e.key === 'ArrowLeft') changePage(-1);
      if (e.key === 'Escape') onClose();
    };
    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [numPages]);

  return (
    <motion.div 
      initial={{ opacity: 0 }}
      animate={{ opacity: 1 }}
      exit={{ opacity: 0 }}
      className="fixed inset-0 z-[100] bg-ink/95 backdrop-blur-md flex flex-col items-center overflow-hidden"
    >
      {/* Top Bar */}
      <header className="w-full bg-ink/50 border-b border-paper/10 px-6 py-4 flex justify-between items-center z-10">
        <div className="flex flex-col">
          <span className="text-[10px] font-sans uppercase tracking-[0.3em] text-clay/60">Digital Volume</span>
          <h2 className="text-paper text-sm font-sans font-bold tracking-widest truncate max-w-md">{title}</h2>
        </div>
        
        <div className="flex items-center gap-6">
          <div className="hidden md:flex items-center gap-4 border-x border-paper/10 px-6">
            <button onClick={() => handleZoom(-0.2)} className="text-paper/60 hover:text-clay transition-colors">
              <ZoomOut className="w-4 h-4" />
            </button>
            <span className="text-[10px] font-sans text-paper/40 w-12 text-center">{Math.round(scale * 100)}%</span>
            <button onClick={() => handleZoom(0.2)} className="text-paper/60 hover:text-clay transition-colors">
              <ZoomIn className="w-4 h-4" />
            </button>
          </div>
          
          <button 
            onClick={onClose}
            className="w-10 h-10 flex items-center justify-center rounded-full bg-paper/5 text-paper hover:bg-clay hover:text-ink transition-all"
          >
            <X className="w-5 h-5" />
          </button>
        </div>
      </header>

      {/* PDF Viewport */}
      <main className="flex-1 w-full overflow-y-auto overflow-x-hidden flex justify-center p-4 md:p-12 scrollbar-thin scrollbar-thumb-clay">
        <div className="relative shadow-[0_30px_60px_-12px_rgba(0,0,0,0.5)] bg-paper origin-top transition-transform duration-300">
          <Document
            file={fileUrl}
            onLoadSuccess={onDocumentLoadSuccess}
            loading={
              <div className="flex flex-col items-center gap-6 py-32 px-48 bg-ink/10">
                <Loader2 className="w-12 h-12 text-clay animate-spin" />
                <span className="font-serif italic text-paper/30 text-xl">Retrieving Pages from the Void...</span>
              </div>
            }
            error={
              <div className="p-12 text-paper/60 font-serif italic text-center">
                The volume could not be accessed. It may have been lost to time or restricted by a Curator.
              </div>
            }
          >
            <Page 
              pageNumber={pageNumber} 
              scale={scale} 
              renderAnnotationLayer={false}
              renderTextLayer={true}
              className="max-w-full"
            />
          </Document>
        </div>
      </main>

      {/* Bottom Pagination */}
      <footer className="w-full bg-ink/80 backdrop-blur-xl border-t border-paper/5 px-6 py-6 flex flex-col items-center gap-4 z-10">
        <div className="flex items-center gap-12">
          <button 
            onClick={() => changePage(-1)}
            disabled={pageNumber <= 1}
            className="text-paper/60 hover:text-clay disabled:opacity-20 transition-all p-2"
          >
            <ChevronLeft className="w-8 h-8" />
          </button>
          
          <div className="flex flex-col items-center gap-1">
            <span className="text-paper font-sans font-bold tracking-[0.2em] text-xs">
              PAGE {pageNumber} <span className="text-paper/30 mx-2">OF</span> {numPages || '...'}
            </span>
            <div className="w-32 h-0.5 bg-paper/10 relative">
              <motion.div 
                className="absolute top-0 left-0 h-full bg-clay"
                initial={{ width: 0 }}
                animate={{ width: `${(pageNumber / (numPages || 1)) * 100}%` }}
              />
            </div>
          </div>

          <button 
            onClick={() => changePage(1)}
            disabled={pageNumber >= numPages}
            className="text-paper/60 hover:text-clay disabled:opacity-20 transition-all p-2"
          >
            <ChevronRight className="w-8 h-8" />
          </button>
        </div>
      </footer>
    </motion.div>
  );
};

export default PdfReader;
