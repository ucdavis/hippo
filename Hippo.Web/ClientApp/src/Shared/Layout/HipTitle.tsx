import React from "react";

interface HipTitleProps {
  title: string;
  subtitle?: string;
  buttons?: React.ReactNode;
}

/**
 *
 * @param title `<h2>`
 * @param subtitle optional, `<h4>`
 * @param buttons optional, adds to the right side of the header
 */
const HipTitle: React.FC<HipTitleProps> = ({ title, subtitle, buttons }) => {
  return (
    <div className="hip-title row justify-content-between align-items-end">
      <div className="col-md-4">
        {subtitle && <h4 className="page-subtitle">{subtitle}</h4>}
        <h2 className="page-title">{title}</h2>
      </div>
      {buttons && (
        <div className="page-title-buttons col-md-8 text-end">{buttons}</div>
      )}
    </div>
  );
};

export default HipTitle;
