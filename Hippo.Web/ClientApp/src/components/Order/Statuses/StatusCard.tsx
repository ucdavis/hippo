import React from "react";
import { OrderStatus, getStatusActions } from "./status";
import { CardSubtitle } from "reactstrap";
import StatusDescription from "./StatusDescription";

interface StatusCardProps {
  status: OrderStatus;
  isAdmin: boolean;
  showStatusActions?: boolean;
}

const StatusCard: React.FC<StatusCardProps> = ({
  status,
  isAdmin,
  showStatusActions = false,
}) => {
  const { sponsorActions, adminActions } = getStatusActions({
    status,
    isAdmin,
  });

  return (
    <>
      <CardSubtitle tag="h4">{status}</CardSubtitle>
      <StatusDescription status={status}>
        {showStatusActions && (
          <p className="text-muted small">
            <>
              {isAdmin && (
                <>
                  {" "}
                  {adminActions?.length > 0
                    ? `While order is in this status, you are able to: ${adminActions}`
                    : "You are not able to take any actions while order is in this status."}
                  <br />
                </>
              )}
              <>
                {sponsorActions?.length > 0
                  ? ` While orders are in this status, ${isAdmin ? "sponsors" : "you"} are able to: ${sponsorActions}`
                  : ` ${isAdmin ? "Sponsors" : "You"} are not able to take any actions while order is in this status.`}
              </>
            </>
          </p>
        )}
      </StatusDescription>
    </>
  );
};

export default StatusCard;
