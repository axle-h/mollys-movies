{{/*
Expand the name of the chart.
*/}}
{{- define "mollys-movies.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Create a default fully qualified app name.
We truncate at 63 chars because some Kubernetes name fields are limited to this (by the DNS naming spec).
If release name contains chart name it will be used as a full name.
*/}}
{{- define "mollys-movies.fullname" -}}
{{- if .Values.fullnameOverride }}
{{- .Values.fullnameOverride | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- $name := default .Chart.Name .Values.nameOverride }}
{{- if contains $name .Release.Name }}
{{- .Release.Name | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- printf "%s-%s" .Release.Name $name | trunc 63 | trimSuffix "-" }}
{{- end }}
{{- end }}
{{- end }}

{{- define "mollys-movies.api.fullname" -}}
{{- printf "%s-api" (include "mollys-movies.fullname" .) | trunc 63 | trimSuffix "-" }}
{{- end }}

{{- define "mollys-movies.ui.fullname" -}}
{{- printf "%s-ui" (include "mollys-movies.fullname" .) | trunc 63 | trimSuffix "-" }}
{{- end }}

{{- define "mollys-movies.scraper.fullname" -}}
{{- printf "%s-scraper" (include "mollys-movies.fullname" .) | trunc 63 | trimSuffix "-" }}
{{- end }}

{{- define "mollys-movies.transmission.fullname" -}}
{{- printf "%s-transmission" (include "mollys-movies.fullname" .) | trunc 63 | trimSuffix "-" }}
{{- end }}

{{- define "mollys-movies.vpn.fullname" -}}
{{- printf "%s-vpn" (include "mollys-movies.fullname" .) | trunc 63 | trimSuffix "-" }}
{{- end }}

{{- define "mollys-movies.mongodbConnectionString" -}}
{{- $url := (printf "%s-mongodb" .Release.Name) | trunc 63 | trimSuffix "-" }}
{{- printf "mongodb://%s:%s@%s:%v/%s"
    (first .Values.mongodb.auth.usernames) (first .Values.mongodb.auth.passwords) $url .Values.mongodb.service.port (first .Values.mongodb.auth.databases) -}}
{{- end }}

{{- define "mollys-movies.rabbitmqConnectionString" -}}
{{- $url := (printf "%s-rabbitmq" .Release.Name) | trunc 63 | trimSuffix "-" }}
{{- printf "rabbitmq://%s:%s@%s:%v"
    .Values.rabbitmq.auth.username .Values.rabbitmq.auth.password $url .Values.rabbitmq.service.port -}}
{{- end }}

{{/*
Create chart name and version as used by the chart label.
*/}}
{{- define "mollys-movies.chart" -}}
{{- printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" }}
{{- end }}

{{- define "mollys-movies.labels" -}}
helm.sh/chart: {{ include "mollys-movies.chart" . }}
app.kubernetes.io/name: {{ include "mollys-movies.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}

{{- define "mollys-movies.api.selectorLabels" -}}
{{- $name := include "mollys-movies.name" . }}
app.kubernetes.io/name: {{ $name }}-api
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}

{{- define "mollys-movies.ui.selectorLabels" -}}
{{- $name := include "mollys-movies.name" . }}
app.kubernetes.io/name: {{ $name }}-ui
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}

{{- define "mollys-movies.scraper.selectorLabels" -}}
{{- $name := include "mollys-movies.name" . }}
app.kubernetes.io/name: {{ $name }}-scraper
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}

{{- define "mollys-movies.transmission.selectorLabels" -}}
{{- $name := include "mollys-movies.name" . }}
app.kubernetes.io/name: {{ $name }}-transmission
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}

{{- define "mollys-movies.vpn.selectorLabels" -}}
{{- $name := include "mollys-movies.name" . }}
app.kubernetes.io/name: {{ $name }}-vpn
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}

{{- define "mollys-movies.api.image" -}}
{{- printf "%s:%s" .Values.api.image.repository (.Values.api.image.tag | default .Chart.AppVersion) | quote -}}
{{- end -}}

{{- define "mollys-movies.ui.image" -}}
{{- printf "%s:%s" .Values.ui.image.repository (.Values.ui.image.tag | default .Chart.AppVersion) | quote -}}
{{- end -}}

{{- define "mollys-movies.scraper.image" -}}
{{- printf "%s:%s" .Values.scraper.image.repository (.Values.scraper.image.tag | default .Chart.AppVersion) | quote -}}
{{- end -}}

{{/*
Create the name of the service account to use
*/}}
{{- define "mollys-movies.serviceAccountName" -}}
{{- default (include "mollys-movies.fullname" .) .Values.serviceAccount.name }}
{{- end }}
