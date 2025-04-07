import { useContext, useEffect, useState } from "react";
import "react-bootstrap-typeahead/css/Typeahead.css";
import { useNavigate, useParams } from "react-router-dom";
import AppContext from "../../Shared/AppContext";
import {
  GroupModel,
  AccountCreateModel,
  AccessType,
  RequestModel,
} from "../../types";
import { authenticatedFetch, parseBadRequest } from "../../util/api";
import { usePromiseNotification } from "../../util/Notifications";
import { GroupLookup } from "../Group/GroupLookup";
import SshKeyInput from "../../Shared/SshKeyInput";
import SearchDefinedOptions from "../../Shared/SearchDefinedOptions";
import HipMainWrapper from "../../Shared/Layout/HipMainWrapper";
import HipTitle from "../../Shared/Layout/HipTitle";
import HipBody from "../../Shared/Layout/HipBody";
import HipButton from "../../Shared/HipComponents/HipButton";

export const RequestForm = () => {
  const [context, setContext] = useContext(AppContext);
  const [notification, setNotification] = usePromiseNotification();

  const navigate = useNavigate();
  const { cluster: clusterName } = useParams();
  const [groups, setGroups] = useState<GroupModel[]>([]);
  const cluster = context.clusters.find((c) => c.name === clusterName);
  const [request, setRequest] = useState<AccountCreateModel>({
    groupId: 0,
    sshKey: "",
    supervisingPI: "",
    accessTypes: [...cluster.accessTypes],
  });

  // load up possible groups
  useEffect(() => {
    const fetchGroups = async () => {
      const response = await authenticatedFetch(
        `/api/${clusterName}/group/groups`,
      );

      const groupsResult = (await response.json()) as GroupModel[];

      if (response.ok) {
        setGroups(groupsResult);
      }
    };

    fetchGroups();
  }, [clusterName]);

  const handleSubmit = async () => {
    const req = authenticatedFetch(`/api/${clusterName}/account/create`, {
      method: "POST",
      body: JSON.stringify(request),
    });

    setNotification(
      req,
      "Saving",
      "Request Created. Please wait for your sponsor to approve your request.",
      async (r) => {
        if (r.status === 400) {
          const errors = await parseBadRequest(response);
          return errors;
        } else {
          return "An error happened, please try again.";
        }
      },
    );

    const response = await req;

    if (response.ok) {
      const requestModel = (await response.json()) as RequestModel;

      setContext((ctx) => ({
        ...ctx,
        openRequests: [...ctx.openRequests, requestModel],
      }));
      navigate(`/${clusterName}/accountstatus`);
    }
  };

  useEffect(() => {
    if (
      context.openRequests.some(
        (r) => r.cluster === clusterName && r.action === "CreateAccount",
      )
    ) {
      // there's already a request for this cluster, redirect to pending page
      navigate(`/${clusterName}/accountstatus`);
    } else if (context.accounts.some((a) => a.cluster === clusterName)) {
      // user already has an account on this cluster, redirect to account info page
      navigate(`/${clusterName}/myaccount`);
    }
  }, [clusterName, context.accounts, context.openRequests, navigate]);

  return (
    <HipMainWrapper>
      <HipTitle
        title={`Welcome, ${context.user.detail.firstName}`}
        subtitle="Request Form"
      />
      <HipBody>
        <p>
          You don't seem to have an account on this cluster yet. If you'd like
          access, please answer the&nbsp;questions&nbsp;below
        </p>
        <hr />
        <div className="form-group">
          <label>
            Who is sponsoring your account? If you are the PI wanting your own
            group, please select the 'Sponsors' group.
          </label>
          <GroupLookup
            setSelection={(group) => {
              if (group) {
                setRequest((r) => ({ ...r, groupId: group.id }));
              }
            }}
            options={groups}
          />
          <p className="form-helper">
            Your group is probably named after your PI or your Department. You
            can filter this list by typing in it. If you don't see your sponsor,
            please contact your PI and ask them to request an account.
          </p>
        </div>
        <div className="form-group">
          <label className="form-label">Who is your supervising PI?</label>
          <input
            className="form-control"
            id="supervisingPI"
            placeholder="Supervising PI"
            value={request.supervisingPI}
            onChange={(e) =>
              setRequest((r) => ({ ...r, supervisingPI: e.target.value }))
            }
          ></input>
          <p className="form-helper">
            Some clusters may require additional clarification on who your
            supervising PI will be. If you are unsure, please ask your sponsor.
          </p>
        </div>
        <div className="form-group">
          <label className="form-label">Access Type</label>
          <SearchDefinedOptions<AccessType>
            definedOptions={cluster.accessTypes}
            selected={request.accessTypes}
            onSelect={(accessTypes) =>
              setRequest((r) => ({ ...r, accessTypes }))
            }
            disabled={false}
            placeHolder="Select one or more access types"
            id="selectAccessTypes"
          />
        </div>
        {cluster.accessTypes.includes("SshKey") &&
          request.accessTypes.includes("SshKey") && (
            <div className="form-group">
              <label className="form-label">
                Please paste your public SSH key.
              </label>
              <SshKeyInput
                onChange={(value) =>
                  setRequest((r) => ({ ...r, sshKey: value }))
                }
              />
            </div>
          )}
        {cluster.acceptableUsePolicyUpdatedOn &&
          cluster.acceptableUsePolicyUrl && (
            <>
              <div className="form-group">
                <label htmlFor="fieldHasAgreedToAup">
                  I have read and agree to abide by the{" "}
                  <a
                    href={cluster.acceptableUsePolicyUrl}
                    target="_blank"
                    rel="noopener noreferrer"
                  >
                    Acceptable Use Policy
                  </a>
                </label>{" "}
                <input
                  id="fieldHasAgreedToAup"
                  type="checkbox"
                  checked={!!request.acceptableUsePolicyAgreedOn}
                  onChange={(e) => {
                    setRequest((r) => ({
                      ...r,
                      acceptableUsePolicyAgreedOn: e.target.checked
                        ? new Date().toISOString()
                        : undefined,
                    }));
                  }}
                />
              </div>
            </>
          )}
        <br />
        <HipButton
          disabled={
            notification.pending ||
            !request.accessTypes.length ||
            // if cluster has a AUP, then requester must agree
            (cluster.acceptableUsePolicyUpdatedOn &&
              cluster.acceptableUsePolicyUrl &&
              !request.acceptableUsePolicyAgreedOn)
          }
          onClick={handleSubmit}
        >
          Submit
        </HipButton>
        {cluster.accessTypes.includes("SshKey") && (
          <div>
            <br />
            <br />
            <p className="form-helper">
              For information on generating a ssh key pair, see this link:{" "}
              <a
                href="https://hpc.ucdavis.edu/faq/access-to-hpc#ssh-key"
                target={"blank"}
              >
                https://hpc.ucdavis.edu/faq/access-to-hpc#ssh-key
              </a>
            </p>
            <p className="form-helper">
              For common issues and questions, see this link:{" "}
              <a href="https://hpc.ucdavis.edu/faq" target={"blank"}>
                https://hpc.ucdavis.edu/faq
              </a>
            </p>
            <p className="form-helper">
              To learn about Linux commands and scripts, see this link:{" "}
              <a
                href="https://hpc.ucdavis.edu/linux-tutorials"
                target={"blank"}
              >
                https://hpc.ucdavis.edu/linux-tutorials
              </a>
            </p>
            <p className="form-helper">
              Data Science Training tutorials:{" "}
              <a
                href="https://ngs-docs.github.io/2021-august-remote-computing/index.html"
                target={"blank"}
              >
                https://ngs-docs.github.io/2021-august-remote-computing/index.html
              </a>
            </p>
          </div>
        )}
      </HipBody>
    </HipMainWrapper>
  );
};
