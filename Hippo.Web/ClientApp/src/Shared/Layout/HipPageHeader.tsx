import React from "react";

interface HipPageHeaderProps {
  title?: string;
  pageName: string;
  buttons?: React.ReactNode;
}

/**
 *
 * @param title: optional, `<h2>` the title specific to the item for the page, e.g. "Order {id}: {name}"
 * @param pageName: `<h4>` the name of the page, e.g. Details, Edit, Admin Orders.
 * @param buttons: optional, adds to the right side of the header
 */
const HipPageHeader: React.FC<HipPageHeaderProps> = ({
  title,
  pageName,
  buttons,
}) => {
  return (
    <div className="row justify-content-between align-items-end">
      <div className="col-md-4">
        <h4 className="page-subtitle">{pageName}</h4>
        {title && <h2 className="page-title">{title}</h2>}
      </div>
      {buttons && (
        <div className="page-title-buttons col-md-8 text-end">{buttons}</div>
      )}
    </div>
  );
};

export default HipPageHeader;
