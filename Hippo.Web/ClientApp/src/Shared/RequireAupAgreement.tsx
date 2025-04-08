import React, { useContext, useMemo, useState } from "react";

import AppContext from "./AppContext";
import { useParams } from "react-router-dom";
import HipBody from "./Layout/HipBody";
import HipMainWrapper from "./Layout/HipMainWrapper";
import HipTitle from "./Layout/HipTitle";
import HipButton from "./HipComponents/HipButton";
import { usePromiseNotification } from "../util/Notifications";
import { authenticatedFetch, parseBadRequest } from "../util/api";

interface Props {
  children: any;
}

export const RequireAupAgreement = (props: Props) => {
  const { children } = props;
  const [context, setContext] = useContext(AppContext);
  const { cluster: clusterName } = useParams();
  const [hasAgreedToAup, setHasAgreedToAup] = useState(false);
  const [notification, setNotification] = usePromiseNotification();
  const hasSystemPermission = context.user.permissions.some(
    (p) => p.role === "System",
  );
  const cluster = context.clusters.find((c) => c.name === clusterName);
  const account = context.accounts.find((a) => a.cluster === clusterName);

  const currentOpenRequests = useMemo(
    () => context.openRequests.filter((r) => r.cluster === clusterName),
    [context.openRequests, clusterName],
  );

  const handleAgreetoAup = async () => {
    const request = authenticatedFetch(
      `/api/${clusterName}/account/agreeToAup/`,
      {
        method: "POST",
      },
    );

    setNotification(
      request,
      "Agreeing to AUP",
      "Agreement Received",
      async (r) => {
        if (r.status === 400) {
          const errors = await parseBadRequest(response);
          return errors;
        } else {
          return "An error happened, please try again.";
        }
      },
    );

    const response = await request;
    if (response.ok) {
      // Updating context here should cause a rerender which will pick up the new agreedOn
      // date and allow the children to be passed through
      setContext((prevContext) => ({
        ...prevContext,
        accounts: prevContext.accounts.map((a) =>
          a.cluster === clusterName
            ? { ...a, acceptableUsePolicyAgreedOn: new Date().toISOString() }
            : a,
        ),
      }));
    }
  };

  if (!clusterName || !cluster) {
    // route params are never available on first render
    return null;
  }

  if (
    hasSystemPermission ||
    !cluster.acceptableUsePolicyUrl ||
    !cluster.acceptableUsePolicyUpdatedOn
  ) {
    // cluster doesn't have an AUP, so no verification necessary
    return <>{children}</>;
  }

  if (!account) {
    const request = currentOpenRequests.find(
      (r) => r.cluster === clusterName && r.action === "CreateAccount",
    );
    if (request) {
      // A pending account creation request implies the AUP has already
      // been agreed to, so it's okay to show the child components
      return <>{children}</>;
    }
  }

  if (
    !account?.acceptableUsePolicyAgreedOn ||
    new Date(account.acceptableUsePolicyAgreedOn) <
      new Date(cluster.acceptableUsePolicyUpdatedOn)
  ) {
    // User has not agreed to the latest AUP
    return (
      <HipMainWrapper>
        <HipTitle
          title={`Welcome, ${context.user.detail.firstName}`}
          subtitle="Acceptable Use Policy"
        />
        <HipBody>
          <p>
            In order to continue accessing this cluster, you need to read and
            agree to the cluster's latest Acceptable Use Policy
          </p>
          <hr />
          <div className="form-group">
            <input
              id="fieldHasAgreedToAup"
              type="checkbox"
              checked={hasAgreedToAup}
              onChange={(e) => {
                setHasAgreedToAup(e.target.checked);
              }}
            />{" "}
            <label htmlFor="fieldHasAgreedToAup">
              I have read and agree to abide by the{" "}
              <a
                href={cluster.acceptableUsePolicyUrl}
                target="_blank"
                rel="noopener noreferrer"
              >
                Acceptable Use Policy
              </a>
            </label>
          </div>{" "}
          <div>
            <br />
            <HipButton
              disabled={notification.pending || !hasAgreedToAup}
              onClick={() => handleAgreetoAup()}
            >
              Agree
            </HipButton>{" "}
          </div>
        </HipBody>
      </HipMainWrapper>
    );
  }

  return <>{children}</>;
};
