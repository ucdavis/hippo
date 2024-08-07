import { useContext } from "react";
import AppContext from "../../Shared/AppContext";
import { useParams } from "react-router-dom";
import { SplitCamelCase } from "../../util/StringHelpers";
import { RequestStatus } from "../../types";
import HipMainWrapper from "../../Shared/Layout/HipMainWrapper";
import HipTitle from "../../Shared/Layout/HipTitle";
import HipBody from "../../Shared/Layout/HipBody";

export const AccountStatus = () => {
  const [context] = useContext(AppContext);
  const { cluster } = useParams();
  const request = context.openRequests.find(
    (r) => r.cluster === cluster && r.action === "CreateAccount",
  );

  return (
    <HipMainWrapper>
      <HipTitle
        title={`Welcome, ${context.user.detail.firstName}`}
        subtitle="Account Status"
      />
      <HipBody>
        {!!request ? (
          <p>
            Your account request for{" "}
            <span className="status-color">{cluster}</span> currently has a
            status of{" "}
            <span className="status-color">
              {SplitCamelCase(request.status)}
            </span>
            .
            {request.status === RequestStatus.PendingApproval && (
              <>
                <br />
                Your sponsor has been emailed and you will be notified when it
                has been acted on.
              </>
            )}
            {request.status === RequestStatus.Processing && (
              <>
                <br />
                Your request has been approved and is in process.
              </>
            )}
            <br />
            Please check back later.
          </p>
        ) : (
          <p>
            You don't appear to have any pending account requests for{" "}
            <span className="status-color">{cluster}</span>.
          </p>
        )}
      </HipBody>
    </HipMainWrapper>
  );
};
