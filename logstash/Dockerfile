FROM docker.elastic.co/logstash/logstash:7.7.0
RUN rm -f /usr/share/logstash/pipeline/logstash.conf
RUN logstash-plugin install logstash-input-file
ADD pipeline/ /usr/share/logstash/pipeline/
ADD config/ /usr/share/logstash/config/
