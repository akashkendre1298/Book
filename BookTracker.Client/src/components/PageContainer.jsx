import React, { useEffect } from 'react';
import { motion } from 'framer-motion';
import { useLocation } from 'react-router-dom';

const PageContainer = ({ children }) => {
  const { pathname } = useLocation();

  useEffect(() => {
    // Force scroll to top on every route change
    window.scrollTo({
      top: 0,
      left: 0,
      behavior: 'instant' // Instant scroll for better UX during transition
    });
  }, [pathname]);

  return (
    <motion.div
      key={pathname} // Ensure animation re-runs on route change
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.8, ease: "easeOut" }}
      className="min-h-screen"
    >
      {children}
    </motion.div>
  );
};

export default PageContainer;
