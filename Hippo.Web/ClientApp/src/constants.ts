import { AccessType } from "./types";

export const AccessTypes = ["SshKey", "OpenOnDemand"] as AccessType[];

// The SoftwareRequestForm issue asked to include clusters that aren't currently in the db, so here we are...
export const ClusterNames = [
  "Farm",
  "Franklin",
  "HPC1",
  "HPC2",
  "LSSC0",
  "Peloton",
] as const;
