import React from "react";
import {
  OrderStatus,
  getStatusActions,
  orderStatusDescriptions,
} from "./status";
import { CardSubtitle, CardText } from "reactstrap";
import StatusDescription from "./StatusDescription";

interface StatusCardProps {
  status: OrderStatus;
  isAdmin: boolean;
  hideAdminDescription?: boolean;
  hideSponsorDescription?: boolean;
  showStatusActions?: boolean;
}

const StatusCard: React.FC<StatusCardProps> = ({
  status,
  isAdmin,
  hideAdminDescription = !isAdmin,
  hideSponsorDescription = isAdmin,
  showStatusActions = false,
}) => {
  const { adminDescription, sponsorDescription } =
    orderStatusDescriptions[status];

  return (
    <>
      <CardSubtitle tag="h4">{status}</CardSubtitle>
      <StatusDescription status={status}>
        {isAdmin && !hideAdminDescription && adminDescription && (
          <>{adminDescription} </>
        )}
        {!hideSponsorDescription && sponsorDescription && (
          <>{sponsorDescription} </>
        )}
        <p className="text-muted small">
          {showStatusActions && (
            <>Available Actions: {getStatusActions(status, isAdmin)}</>
          )}
        </p>
      </StatusDescription>
    </>
  );
};

export default StatusCard;
