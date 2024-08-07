import React from "react";

interface HipPageHeaderProps {
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
const HipPageHeader: React.FC<HipPageHeaderProps> = ({
  title,
  subtitle,
  buttons,
}) => {
  return (
    <div className="hip-header row justify-content-between align-items-end">
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

export default HipPageHeader;
