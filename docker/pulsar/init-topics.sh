#!/usr/bin/env bash

set -Eeuo pipefail

readonly ADMIN_URL="${PULSAR_ADMIN_URL:?PULSAR_ADMIN_URL is required}"
readonly MAIN_TOPIC="${PULSAR_TOPIC:?PULSAR_TOPIC is required}"
readonly PARTITION_COUNT="${PULSAR_PARTITIONS:-4}"

pulsar_admin() {
  /pulsar/bin/pulsar-admin \
    --admin-url "${ADMIN_URL}" \
    "$@"
}

create_partitioned_topic_if_missing() {
  local topic="$1"
  local partitions="$2"

  if pulsar_admin \
    topics get-partitioned-topic-metadata \
    "${topic}" > /dev/null 2>&1
  then
    echo "Partitioned topic already exists: ${topic}"
    return
  fi

  pulsar_admin \
    topics create-partitioned-topic \
    "${topic}" \
    --partitions "${partitions}"

  echo "Created partitioned topic: ${topic}"
}

create_topic_if_missing() {
  local topic="$1"

  if pulsar_admin topics stats "${topic}" > /dev/null 2>&1
  then
    echo "Topic already exists: ${topic}"
    return
  fi

  pulsar_admin topics create "${topic}"

  echo "Created topic: ${topic}"
}

create_dead_letter_topics() {
  local services=(
    "orders"
    "inventory"
    "payments"
  )

  for service in "${services[@]}"
  do
    create_topic_if_missing \
      "${MAIN_TOPIC}-${service}-dlq"
  done
}

create_partitioned_topic_if_missing \
  "${MAIN_TOPIC}" \
  "${PARTITION_COUNT}"

create_dead_letter_topics

echo "Pulsar topic initialization completed."