import React from "react";
import { Progress } from "reactstrap";
import { OrderStatus, compareOrderStatus } from "./status";
import HipProgress from "../../../Shared/HipComponents/HipProgress";
import StatusDescription from "./StatusDescription";

interface StatusBarProps {
  hideTooltip?: boolean;
  max?: number;
  status: OrderStatus;
  animated?: boolean;
  showOnHover?: OrderStatus | null;
  creatingForPI?: boolean;
}

const StatusBar: React.FC<StatusBarProps> = ({
  hideTooltip = false,
  max = 5,
  status,
  animated,
  showOnHover,
  creatingForPI = false,
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
              !hideTooltip && (
                <StatusDescription status={OrderStatus.Cancelled} />
              )
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
              !hideTooltip && (
                <StatusDescription status={OrderStatus.Rejected} />
              )
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
                  status === OrderStatus.Draft
                }
                animated={status === OrderStatus.Draft && animated}
                tooltip={
                  !hideTooltip && (
                    <StatusDescription
                      status={
                        status === OrderStatus.Draft
                          ? OrderStatus.Draft
                          : OrderStatus.Created
                      }
                    />
                  )
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
                  (status === OrderStatus.Draft && !creatingForPI)
                }
                animated={status === OrderStatus.Created && animated}
                tooltip={
                  !hideTooltip && (
                    <StatusDescription status={OrderStatus.Submitted} />
                  )
                }
              />
              <HipProgress
                id="status-bar-processing"
                label={OrderStatus.Processing}
                max={max}
                shouldFill={
                  compareOrderStatus(status, OrderStatus.Processing) >= 0
                }
                striped={showOnHover === OrderStatus.Processing}
                animated={status === OrderStatus.Submitted && animated}
                tooltip={
                  !hideTooltip && (
                    <StatusDescription status={OrderStatus.Processing} />
                  )
                }
              />
              <HipProgress
                id="status-bar-active"
                label={OrderStatus.Active}
                max={max}
                shouldFill={compareOrderStatus(status, OrderStatus.Active) >= 0}
                striped={showOnHover === OrderStatus.Active}
                animated={status === OrderStatus.Processing && animated}
                tooltip={
                  !hideTooltip && (
                    <StatusDescription status={OrderStatus.Active} />
                  )
                }
              />
              <HipProgress
                id="status-bar-completed"
                label={OrderStatus.Completed}
                max={max}
                shouldFill={
                  compareOrderStatus(status, OrderStatus.Completed) >= 0
                }
                striped={showOnHover === OrderStatus.Completed}
                animated={status === OrderStatus.Active && animated}
                tooltip={
                  !hideTooltip && (
                    <StatusDescription status={OrderStatus.Completed} />
                  )
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
            striped={showOnHover === OrderStatus.Archived}
            tooltip={
              !hideTooltip && (
                <StatusDescription status={OrderStatus.Archived} />
              )
            }
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
              striped={showOnHover === OrderStatus.Closed}
              tooltip={
                !hideTooltip && (
                  <StatusDescription status={OrderStatus.Closed} />
                )
              }
            />
          )}
      </Progress>
    </div>
  );
};

export default StatusBar;
