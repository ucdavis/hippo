import React from "react";
import { Progress } from "reactstrap";
import { OrderStatus, compareOrderStatus } from "../../../types/status";
import HipProgress from "../../../Shared/HipProgress";
import StatusDescription from "../StatusDescription";

interface StatusBarProps {
  isAdmin: boolean;
  max?: number;
  status: OrderStatus;
  animated?: boolean;
  showInProgress?: boolean;
  showOnHover?: OrderStatus | null;
}

const StatusBar: React.FC<StatusBarProps> = ({
  isAdmin,
  max = 5,
  status,
  animated,
  showInProgress,
  showOnHover,
}) => {
  return (
    <div className="hip-progress status-bar">
      <h4>Order Status:</h4>
      <Progress multi>
        {(status === OrderStatus.Cancelled ||
          showOnHover === OrderStatus.Cancelled) && (
          <HipProgress
            id="status-bar-cancelled"
            label={OrderStatus.Cancelled}
            max={max}
            value={max / 2}
            color="danger"
            shouldFill={true}
            striped={showOnHover === OrderStatus.Cancelled}
            animated={false}
            tooltip={
              <StatusDescription
                status={OrderStatus.Cancelled}
                isAdmin={isAdmin}
              />
            }
          />
        )}
        {(status === OrderStatus.Rejected ||
          showOnHover === OrderStatus.Rejected) && (
          <HipProgress
            id="status-bar-rejected"
            label={OrderStatus.Rejected}
            max={max}
            value={max}
            color="danger"
            shouldFill={true}
            striped={showOnHover === OrderStatus.Rejected}
            animated={false}
            tooltip={
              <StatusDescription
                status={OrderStatus.Rejected}
                isAdmin={isAdmin}
              />
            }
          />
        )}
        {status !== OrderStatus.Cancelled &&
          showOnHover !== OrderStatus.Cancelled &&
          status !== OrderStatus.Rejected &&
          showOnHover !== OrderStatus.Rejected &&
          status !== OrderStatus.Archived &&
          showOnHover !== OrderStatus.Archived &&
          status !== OrderStatus.Closed &&
          showOnHover !== OrderStatus.Closed && (
            <>
              <HipProgress
                id="status-bar-draft"
                label={OrderStatus.Created}
                max={max}
                shouldFill={
                  compareOrderStatus(status, OrderStatus.Created) >= 0
                }
                striped={
                  showOnHover === OrderStatus.Created ||
                  (status === OrderStatus.Draft && showInProgress)
                }
                animated={status === OrderStatus.Draft && animated}
                tooltip={
                  <StatusDescription
                    status={
                      status === OrderStatus.Draft
                        ? OrderStatus.Draft
                        : OrderStatus.Created
                    }
                    isAdmin={isAdmin}
                  />
                }
              />
              <HipProgress
                id="status-bar-submitted"
                label={OrderStatus.Submitted}
                max={max}
                shouldFill={
                  compareOrderStatus(status, OrderStatus.Submitted) >= 0
                }
                striped={
                  showOnHover === OrderStatus.Submitted ||
                  (status === OrderStatus.Created && showInProgress)
                }
                animated={status === OrderStatus.Created && animated}
                tooltip={
                  <StatusDescription
                    status={OrderStatus.Submitted}
                    isAdmin={isAdmin}
                  />
                }
              />
              <HipProgress
                id="status-bar-processing"
                label={OrderStatus.Processing}
                max={max}
                shouldFill={
                  compareOrderStatus(status, OrderStatus.Processing) >= 0
                }
                striped={
                  showOnHover === OrderStatus.Processing ||
                  (status === OrderStatus.Submitted && showInProgress)
                }
                animated={status === OrderStatus.Submitted && animated}
                tooltip={
                  <StatusDescription
                    status={OrderStatus.Processing}
                    isAdmin={isAdmin}
                  />
                }
              />
              <HipProgress
                id="status-bar-active"
                label={OrderStatus.Active}
                max={max}
                shouldFill={compareOrderStatus(status, OrderStatus.Active) >= 0}
                striped={
                  showOnHover === OrderStatus.Active ||
                  (status === OrderStatus.Processing && showInProgress)
                }
                animated={status === OrderStatus.Processing && animated}
                tooltip={
                  <StatusDescription
                    status={OrderStatus.Active}
                    isAdmin={isAdmin}
                  />
                }
              />
              <HipProgress
                id="status-bar-completed"
                label={OrderStatus.Completed}
                max={max}
                shouldFill={
                  compareOrderStatus(status, OrderStatus.Completed) >= 0
                }
                striped={
                  showOnHover === OrderStatus.Completed ||
                  (status === OrderStatus.Active && showInProgress)
                }
                animated={status === OrderStatus.Active && animated}
                tooltip={
                  <StatusDescription
                    status={OrderStatus.Completed}
                    isAdmin={isAdmin}
                  />
                }
              />
            </>
          )}
        {(status === OrderStatus.Archived ||
          showOnHover === OrderStatus.Archived) && (
          <HipProgress
            id="status-bar-archived"
            label={OrderStatus.Archived}
            max={max}
            value={max}
            color="secondary"
            shouldFill={true}
            inProgress={showOnHover === OrderStatus.Archived}
            striped={false}
          />
        )}

        {showOnHover !== OrderStatus.Archived &&
          (status === OrderStatus.Closed ||
            showOnHover === OrderStatus.Closed) && (
            <HipProgress
              id="status-bar-closed"
              label={OrderStatus.Closed}
              max={max}
              value={max}
              color="secondary"
              shouldFill={true}
              inProgress={showOnHover === OrderStatus.Closed}
              striped={false}
            />
          )}
      </Progress>
    </div>
  );
};

export default StatusBar;
